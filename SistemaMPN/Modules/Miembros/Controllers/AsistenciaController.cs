using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SistemaMPN.Data;
using SistemaMPN.Modules.Autentificacion.Models;
using SistemaMPN.Modules.Autentificacion.Services;
using SistemaMPN.Modules.Miembros.Services;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Miembros.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AsistenciaController : ControllerBase
{
    private readonly IAsistenciaService _asistenciaService;
    private readonly ILogger<AsistenciaController> _logger;
    private readonly DataContext _context;
    private readonly IEmailService _emailService;

    public AsistenciaController(IAsistenciaService asistenciaService, ILogger<AsistenciaController> logger, DataContext context, IEmailService emailService)
    {
        _asistenciaService = asistenciaService;
        _logger = logger;
        _context = context;
        _emailService = emailService;
    }

    /// <summary>
    /// Genera código de verificación para un evento (requiere auth)
    /// </summary>
    [HttpGet("GenerarQR/{eventoId}")]
    [Authorize]
    public async Task<ActionResult<QrEventoDTO>> GenerarQR(int eventoId)
    {
        try
        {
            var qrDto = await _asistenciaService.GenerarCodigoEventoAsync(eventoId);
            return Ok(qrDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar código QR para el evento {EventoId}", eventoId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Genera imagen QR en formato PNG (requiere auth)
    /// </summary>
    [HttpGet("QrImagen/{eventoId}")]
    [Authorize]
    public async Task<ActionResult> QrImagen(int eventoId)
    {
        try
        {
            var qrDto = await _asistenciaService.GenerarCodigoEventoAsync(eventoId);
            var urlCompleta = $"{Request.Scheme}://{Request.Host}{qrDto.UrlCheckIn}";

            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(urlCompleta, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeAsPngBytes = qrCode.GetGraphic(20);

            return File(qrCodeAsPngBytes, "image/png");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar imagen QR para el evento {EventoId}", eventoId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Registra asistencia (público - sin autenticación)
    /// </summary>
    [HttpPost("Registrar")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> RegistrarAsistencia(RegistroAsistenciaDTO registro)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var resultado = await _asistenciaService.RegistrarAsistenciaAsync(registro);

            if (resultado.StartsWith("DUPLICADO:"))
                return Conflict(resultado);

            if (resultado.StartsWith("ERROR:"))
                return BadRequest(resultado);

            if (!string.IsNullOrWhiteSpace(registro.Email))
            {
                await EnviarEmailConfirmacionAsync(registro);
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar asistencia para evento {CodigoEvento}", registro.CodigoEvento);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    private async Task EnviarEmailConfirmacionAsync(RegistroAsistenciaDTO registro)
    {
        try
        {
            var partesCodigo = registro.CodigoEvento.Split('_');
            if (partesCodigo.Length < 2 || !int.TryParse(partesCodigo[1], out int eventoId))
                return;

            var evento = await _context.Eventos.FindAsync(eventoId);
            if (evento == null)
                return;

            var fechaFormateada = evento.Fecha?.ToString("dd/MM/yyyy") ?? "Sin fecha";
            var horaInicio = evento.HoraInicio.HasValue ? evento.HoraInicio.Value.ToString(@"hh\:mm") : "";
            var horaFin = evento.HoraFin.HasValue ? evento.HoraFin.Value.ToString(@"hh\:mm") : "";
            var horario = !string.IsNullOrEmpty(horaInicio) ? $"{horaInicio} - {horaFin}" : "Sin horario";

            var mensaje = $@"
                <h2>Confirmación de Asistencia</h2>
                <p>¡Gracias por registrar tu asistencia!</p>
                <h3>Detalles del evento:</h3>
                <ul>
                    <li><strong>Evento:</strong> {evento.Titulo}</li>
                    <li><strong>Fecha:</strong> {fechaFormateada}</li>
                    <li><strong>Horario:</strong> {horario}</li>
                    <li><strong>Lugar:</strong> {evento.Lugar ?? "Por confirmar"}</li>
                </ul>
                <p>Te esperamos.</p>
                <hr>
                <p style='font-size: 12px; color: #666;'>
                    Este correo fue enviado a {registro.Email}. Si no te inscribiste, ignora este mensaje.
                </p>
            ";

            var emailRequest = new EmailRequest
            {
                To = registro.Email,
                Subject = $"Confirmación de Asistencia: {evento.Titulo}",
                Message = mensaje
            };

            await _emailService.SendEmailAsync(emailRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email de confirmación de asistencia");
        }
    }

    /// <summary>
    /// Obtiene lista de asistentes a un evento (requiere auth)
    /// </summary>
    [HttpGet("GetAsistentes/{eventoId}")]
    [Authorize]
    public async Task<ActionResult<List<AsistenciaDTO>>> GetAsistentes(int eventoId)
    {
        try
        {
            var asistentes = await _asistenciaService.GetAsistentesEventoAsync(eventoId);
            return Ok(asistentes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener asistentes del evento {EventoId}", eventoId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene historial de asistencia de un miembro (requiere auth)
    /// </summary>
    [HttpGet("GetHistorialMiembro/{miembroId}")]
    [Authorize(Roles = "admin,gestor de miembros,lider,auditor")]
    public async Task<ActionResult<List<AsistenciaDTO>>> GetHistorialMiembro(
        int miembroId,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        try
        {
            var historial = await _asistenciaService.GetHistorialMiembroAsync(miembroId, desde, hasta);
            return Ok(historial);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial del miembro {MiembroId}", miembroId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene cantidad de asistentes a un evento (público - sin autenticación)
    /// </summary>
    [HttpGet("GetCantidadAsistentes/{eventoId}")]
    [AllowAnonymous]
    public async Task<ActionResult<int>> GetCantidadAsistentes(int eventoId)
    {
        try
        {
            var cantidad = await _asistenciaService.GetCantidadAsistentesAsync(eventoId);
            return Ok(cantidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cantidad de asistentes del evento {EventoId}", eventoId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Verifica si una persona ya registró asistencia
    /// </summary>
    [HttpGet("Verificar")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> VerificarAsistencia(
        [FromQuery] int? miembroId,
        [FromQuery] string? email,
        [FromQuery] int eventoId)
    {
        try
        {
            var yaRegistro = await _asistenciaService.YaRegistroAsync(miembroId, email, eventoId);
            return Ok(yaRegistro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar asistencia para evento {EventoId}", eventoId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Busca un miembro por DNI y/o email para autocompletar el formulario (público)
    /// </summary>
    [HttpGet("BuscarMiembro")]
    [AllowAnonymous]
    public async Task<ActionResult<InfoBasicaMiembroDTO>> BuscarMiembro(
        [FromQuery] string? dni,
        [FromQuery] string? email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dni) && string.IsNullOrWhiteSpace(email))
                return BadRequest("Se requiere DNI o email para buscar.");

            Miembro? miembro = null;

            if (!string.IsNullOrWhiteSpace(dni))
            {
                miembro = await _context.Miembros
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Dni == dni);
            }

            if (miembro == null && !string.IsNullOrWhiteSpace(email))
            {
                miembro = await _context.Miembros
                    .AsNoTracking()
                    .Include(m => m.Usuario)
                    .FirstOrDefaultAsync(m => m.Usuario != null && m.Usuario.Correo == email);
            }

            if (miembro == null)
                return NotFound("Miembro no encontrado.");

            return Ok(new InfoBasicaMiembroDTO
            {
                nombre = miembro.Nombre,
                apellido = miembro.Apellido,
                dni = miembro.Dni
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar miembro por DNI={Dni}, email={Email}", dni, email);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
