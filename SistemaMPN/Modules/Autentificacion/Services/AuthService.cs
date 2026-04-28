using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaMPN.Controllers;
using SistemaMPN.Modules.Autentificacion.Models;
using SistemaMPN.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SistemaMPN.Modules.Autentificacion.Services
{
    public class AuthService : IAuthService
    {
        private IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ReCaptchaSettings _reCaptchaSettings;
        private readonly ILogger<UsuarioController> _logger;

        public AuthService(IConfiguration configuration,
                           IHttpClientFactory httpClientFactory,
                           IOptions<ReCaptchaSettings> reCaptchaSettings,
                           ILogger<UsuarioController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _reCaptchaSettings = reCaptchaSettings.Value;
            _logger = logger;
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public string CreateToken(Usuario user, string [] roles)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuarios.ToString())
            };
            foreach (var rol in roles)
            {
                //roles
                claims.Add(new Claim(ClaimTypes.Role, rol.ToLower()));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpirationMinutes"])),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public async Task<RecaptchaResponse> ValidateRecaptcha(string token)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={_reCaptchaSettings.SecretKey}&response={token}");

                // Verificar si la respuesta es exitosa
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error en la respuesta de reCAPTCHA: {response.StatusCode}");
                    return new RecaptchaResponse { Success = false };
                }

                // Leer el contenido como string primero para debuggear
                var responseContent = await response.Content.ReadAsStringAsync();

                // Verificar si es HTML inesperado
                if (responseContent.TrimStart().StartsWith("<"))
                {
                    //_logger.LogError($"Respuesta HTML inesperada: {responseContent}");
                    return new RecaptchaResponse { Success = false };
                }

                // Deserializar el JSON
                try
                {
                    var result = JsonSerializer.Deserialize<RecaptchaResponse>(responseContent);
                    //_logger.LogInformation($"Respuesta reCAPTCHA: {JsonSerializer.Serialize(result)}");
                    return result;
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, $"Error al deserializar: {responseContent}");
                    return new RecaptchaResponse { Success = false };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar reCAPTCHA");
                return new RecaptchaResponse { Success = false };
            }
        }
    }
}
