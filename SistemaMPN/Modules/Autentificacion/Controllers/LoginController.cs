using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Controllers;
using SistemaMPN.Data;
using SistemaMPN.Modules.Autentificacion.Models;
using SistemaMPN.Modules.Autentificacion.Services;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Autentificacion.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<UsuarioController> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public LoginController(DataContext context, 
            IAuthService authService,
            ILogger<UsuarioController> logger,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost("Loguear")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> IniciarSesion(UsuarioDTO usuario)
        {
            try
            {
                // Validación del modelo
                if (string.IsNullOrEmpty(usuario.recaptcha_token))
                {
                    return BadRequest("Token reCAPTCHA no proporcionado");
                }

                // Validar reCAPTCHA
                var recaptchaResponse = await _authService.ValidateRecaptcha(usuario.recaptcha_token);

                if (!recaptchaResponse.Success || recaptchaResponse.Score < 0.5)
                {
                    _logger.LogWarning($"Intento de login fallido - reCAPTCHA inválido. Score: {recaptchaResponse.Score}");
                    return BadRequest("Validación de seguridad fallida. Intente nuevamente.");
                }

                // Buscar usuario
                var cuenta = await _context.Usuarios
                    .AsNoTracking()
                    .Where(x => x.UserName == usuario.user_name)
                    .Include(a => a.IdRols)
                    .FirstOrDefaultAsync();

                if (cuenta == null)
                {
                    _logger.LogWarning($"Intento de login fallido - Usuario no encontrado: {usuario.user_name}");
                    return BadRequest("Usuario y/o contraseña incorrectos");
                }

                // Verificar contraseña
                bool valido = _authService.VerifyPasswordHash(usuario.password, cuenta.PasswordHash, cuenta.PasswordSalt);

                if (!valido)
                {
                    _logger.LogWarning($"Intento de login fallido - Contraseña incorrecta para usuario: {usuario.user_name}");
                    return BadRequest("Usuario y/o contraseña incorrectos");
                }

                // Crear token JWT
                var roles = cuenta.IdRols.Select(m => m.Nombre.ToLower()).ToArray();
                string token = _authService.CreateToken(cuenta, roles);

                _logger.LogInformation($"Login exitoso para usuario: {usuario.user_name}");
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el proceso de login");
                return StatusCode(500, "Error interno durante el inicio de sesión");
            }
        }

        [HttpPost("ContraseniaOlvidada")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> contraseniaOlvidada(ForgotPasswordDTO model)
        {
            if (string.IsNullOrEmpty(model.Correo))
            {
                return BadRequest("Email incorrecto");
            }

            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Correo == model.Correo);
            if (usuario == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            //Obtener la peticion para resetear la contraseña en caso de que un usuario ya tenga una asignada
            var peticionCambio = await _context.CambioContrasenias.FirstOrDefaultAsync(x => x.IdUsuarios == usuario.IdUsuarios);

            if(peticionCambio != null && peticionCambio.ResetTokenExpire > DateTime.UtcNow)
            {
                return BadRequest("Ya existe una petición de cambio de contraseña activa para este usuario, revise su correo");
            }

            //Generar un token
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
            DateTime? ResetTokenExpire  = DateTime.UtcNow.AddMinutes(5);

            var cambioContrasenia = new PeticionCambiarPassword
            {
                ResetToken = token,
                ResetTokenExpire = ResetTokenExpire,
                IdUsuarios = usuario.IdUsuarios
            };

            _context.CambioContrasenias.Add(cambioContrasenia);

            await _context.SaveChangesAsync();

            //Codifica el token para que sea seguro utilizarlo en la url
            var tokenWeb = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            string url = $"{_configuration["EmailSettings:AppUrl"]}/NuevaContrasenia?token={tokenWeb}";

            var emailRequest = new EmailRequest
            {
                To = model.Correo!,
                Subject = "Ingresa al siguiente correo",
                Message = $@"<!DOCTYPE html>
                        <html>
                        <head>
                            <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                        </head>
                        <body style=""font-family: Arial, sans-serif; margin: 0; padding: 20px;"">
                            <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                <tr>
                                    <td>
                                        <h2>Cambiar contraseña</h2>
                                        <p>Para restablecer tu contraseña:</p>
                                        <table border=""0"" cellpadding=""0"" cellspacing=""0"">
                                            <tr>
                                                <td style=""padding: 10px 0;"">
                                                    <a href=""{url}"" style=""background-color: #0066cc; color: white; padding: 10px 15px; text-decoration: none; border-radius: 5px; display: inline-block;"">Cliquea aquí</a>
                                                </td>
                                            </tr>
                                        </table>
                                        <p>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                                        <p>{url}</p>
                                    </td>
                                </tr>
                            </table>
                        </body>
                        </html>"
            };

            await _emailService.SendEmailAsync(emailRequest);
            return Ok("Correo enviado para recuperar contrasenia");
        }

        [HttpPatch("UpdatePassword")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> UpdatePassword(ResetPasswordDTO model)
        {
            string token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.ResetToken));

            var cambioContrasenia = await _context.CambioContrasenias
                                        .Include(u => u.IdUsuariosNavigation)
                                        .FirstOrDefaultAsync(x => x.ResetToken == token);


            if (cambioContrasenia == null || cambioContrasenia.ResetTokenExpire < DateTime.Now)
            {
                return BadRequest("Peticion de cambio de contrasenia no encontrada o expirada");
            }

            if (cambioContrasenia.IdUsuariosNavigation == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            _authService.CreatePasswordHash(model.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

            cambioContrasenia.IdUsuariosNavigation.PasswordHash = passwordHash;
            cambioContrasenia.IdUsuariosNavigation.PasswordSalt = passwordSalt;


            await _context.SaveChangesAsync();


            return Ok("Contraseña cambiada exitosamente");
        }
    }
}
