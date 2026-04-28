using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Modules.Autentificacion.Models;
using SistemaMPN.Modules.Autentificacion.Services;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Consultoria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles = "admin, consultor, auditor")]
    public class ReunionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReunionController> _logger;
        public ReunionController(DataContext context, IEmailService emailService, ILogger<ReunionController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("GetReuniones")]
        public async Task<IActionResult> GetReuniones()
        {
            try
            {
                var reuniones = await _context.Reuniones
                    .Include(r => r.Miembro)
                    .Include(r => r.Consultor)
                        .ThenInclude(c => c.IdMiembrosNavigation) // ← Asegura que se cargue la navegación
                    .Select(r => new ReunionDTO
                    {
                        IdReunion = r.IdReunion,
                        Fecha = DateTime.SpecifyKind(r.Fecha, DateTimeKind.Utc), // ← Convertir a UTC
                        Motivo = r.Motivo,
                        Correo = r.Correo,
                        Estado = r.Estado,
                        IdMiembro = r.IdMiembro,
                        IdConsultor = r.IdConsultor,
                        NombreMiembro = r.Miembro != null
                            ? r.Miembro.Nombre + " " + r.Miembro.Apellido
                            : null,
                        NombreConsultor = r.Consultor != null && r.Consultor.IdMiembrosNavigation != null
                            ? r.Consultor.IdMiembrosNavigation.Nombre + " " + r.Consultor.IdMiembrosNavigation.Apellido
                            : null
                    })
                    .OrderByDescending(r => r.Fecha)
                    .ToListAsync();

                return Ok(reuniones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las reuniones");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        [HttpPost("CrearReunion")]
        public async Task<IActionResult> CrearReunion(ReunionDTO reunionDto)
        {
            try
            {
                // Validar que el miembro existe
                var miembroExiste = await _context.Miembros
                    .AnyAsync(m => m.IdMiembros == reunionDto.IdMiembro);
                if (!miembroExiste)
                {
                    return NotFound("El miembro especificado no existe");
                }

                // Validar que el consultor existe
                var consultorExiste = await _context.Consultores
                    .AnyAsync(c => c.IdMiembros == reunionDto.IdConsultor);
                if (!consultorExiste)
                {
                    return NotFound("El consultor especificado no existe");
                }

                // Convertir fecha a UTC para PostgreSQL
                var auxFecha = reunionDto.Fecha;
                var fechaUtc = DateTime.SpecifyKind(reunionDto.Fecha.Value, DateTimeKind.Local).ToUniversalTime();

                // Validar que la fecha no sea en el pasado
                if (fechaUtc < DateTime.UtcNow)
                {
                    return BadRequest("La fecha de la reunión no puede ser en el pasado");
                }

                var reunion = new Reunion
                {
                    Fecha = fechaUtc, // Usar la fecha en UTC
                    Motivo = reunionDto.Motivo,
                    Correo = reunionDto.Correo,
                    Estado = "Programada",
                    IdMiembro = reunionDto.IdMiembro,
                    IdConsultor = reunionDto.IdConsultor,
                    IdPeticion = reunionDto.IdPeticion
                };

                _context.Reuniones.Add(reunion);
                
                // Si la reunión viene de una petición de líder, actualizar estado de la petición
                if (reunionDto.IdPeticion.HasValue && reunionDto.IdPeticion.Value > 0)
                {
                    var peticion = await _context.Peticiones.FindAsync(reunionDto.IdPeticion.Value);
                    if (peticion != null)
                    {
                        peticion.Estado = "Aceptada";
                        peticion.FechaRespuesta = DateTime.UtcNow;
                    }
                }
                
                await _context.SaveChangesAsync();

                var emailRequest = new EmailRequest
                {
                    To = reunionDto.Correo!,
                    Subject = "Reunión programada",
                    Message = $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
</head>
<body style=""font-family: Arial, sans-serif; background-color: #f6f6f6; padding: 20px;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""20"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 6px;"">
                    <tr>
                        <td>
                            <h2 style=""color: #333;"">Reunión programada</h2>

                            <p>Se ha programado una nueva reunión con los siguientes detalles:</p>

                            <table width=""100%"" cellpadding=""5"" cellspacing=""0"">
                                <tr>
                                    <td><strong>Fecha:</strong></td>
                                    <td>{auxFecha:dddd dd/MM/yyyy}</td>
                                </tr>
                                <tr>
                                    <td><strong>Hora:</strong></td>
                                    <td>{auxFecha:HH:mm} hs</td>
                                </tr>
                                <tr>
                                    <td><strong>Motivo:</strong></td>
                                    <td>{reunion.Motivo}</td>
                                </tr>
                                <tr>
                                    <td><strong>Estado:</strong></td>
                                    <td>{reunion.Estado}</td>
                                </tr>
                            </table>

                            <p style=""margin-top: 20px;"">
                                En caso de no poder asistir o necesitar reprogramar la reunión,
                                por favor comuníquese a la brevedad.
                            </p>

                            <p style=""color: #666; font-size: 12px; margin-top: 30px;"">
                                Este es un mensaje automático, por favor no responda a este correo.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>"
                };

                await _emailService.SendEmailAsync(emailRequest);

                // Enviar copia al líder si la reunión viene de una petición
                if (reunionDto.IdPeticion.HasValue && reunionDto.IdPeticion.Value > 0)
                {
                    var peticion = await _context.Peticiones
                        .Include(p => p.IdLiderNavigation)
                            .ThenInclude(l => l.Usuario)
                        .Include(p => p.PeticionReunion)
                            .ThenInclude(pr => pr!.IdMiembrosNavigation)
                        .FirstOrDefaultAsync(p => p.IdPeticiones == reunionDto.IdPeticion.Value);
                    
                    if (peticion != null && peticion.IdLiderNavigation?.Usuario?.Correo != null)
                    {
                        var emailLider = new EmailRequest
                        {
                            To = peticion.IdLiderNavigation.Usuario.Correo,
                            Subject = "Su solicitud de reunión ha sido aceptada",
                            Message = $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
</head>
<body style=""font-family: Arial, sans-serif; background-color: #f6f6f6; padding: 20px;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""20"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 6px;"">
                    <tr>
                        <td>
                            <h2 style=""color: #333;"">Solicitud de reunión aceptada</h2>

                            <p>Su solicitud de reunión ha sido aceptada. Los detalles son:</p>

                            <table width=""100%"" cellpadding=""5"" cellspacing=""0"">
                                <tr>
                                    <td><strong>Fecha:</strong></td>
                                    <td>{auxFecha:dddd dd/MM/yyyy}</td>
                                </tr>
                                <tr>
                                    <td><strong>Hora:</strong></td>
                                    <td>{auxFecha:HH:mm} hs</td>
                                </tr>
                                <tr>
                                    <td><strong>Motivo:</strong></td>
                                    <td>{reunion.Motivo}</td>
                                </tr>
                                <tr>
                                    <td><strong>Miembro:</strong></td>
                                    <td>{peticion.PeticionReunion?.IdMiembrosNavigation?.Nombre} {peticion.PeticionReunion?.IdMiembrosNavigation?.Apellido}</td>
                                </tr>
                            </table>

                            <p style=""margin-top: 20px;"">
                                El miembro ha sido notificado sobre la reunión programada.
                            </p>

                            <p style=""color: #666; font-size: 12px; margin-top: 30px;"">
                                Este es un mensaje automático, por favor no responda a este correo.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>"
                        };

                        try
                        {
                            await _emailService.SendEmailAsync(emailLider);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error al enviar correo de notificación al líder");
                        }
                    }
                }

                var response = new ReunionDTO
                {
                    IdReunion = reunion.IdReunion,
                    Fecha = reunion.Fecha,
                    Motivo = reunion.Motivo,
                    Correo = reunion.Correo,
                    Estado = reunion.Estado,
                    IdMiembro = reunion.IdMiembro,
                    IdConsultor = reunion.IdConsultor
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la reunión");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("GetReunion/{id}")]
        public async Task<IActionResult> GetReunion(int id)
        {
            try
            {
                var reunion = await _context.Reuniones
                    .Include(r => r.Miembro)
                    .Include(r => r.Consultor)
                        .ThenInclude(c => c.IdMiembrosNavigation)
                    .FirstOrDefaultAsync(r => r.IdReunion == id);

                if (reunion == null)
                {
                    return NotFound("Reunión no encontrada");
                }

                var reunionDto = new ReunionDTO
                {
                    IdReunion = reunion.IdReunion,
                    Fecha = reunion.Fecha,
                    Motivo = reunion.Motivo,
                    Correo = reunion.Correo,
                    Estado = reunion.Estado,
                    IdMiembro = reunion.IdMiembro,
                    IdConsultor = reunion.IdConsultor,
                    NombreMiembro = reunion.Miembro != null
                        ? $"{reunion.Miembro.Nombre} {reunion.Miembro.Apellido}"
                        : null,
                    NombreConsultor = reunion.Consultor?.IdMiembrosNavigation != null  
                        ? $"{reunion.Consultor.IdMiembrosNavigation.Nombre} {reunion.Consultor.IdMiembrosNavigation.Apellido}"
                        : null
                };

                return Ok(reunionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la reunión");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }
        [HttpPut("CompletarReunion/{id}")]
        public async Task<IActionResult> CompletarReunion(int id)
        {
            try
            {
                var reunion = await _context.Reuniones.FindAsync(id);
                switch (reunion)
                {
                    case null:
                        return NotFound("Reunión no encontrada");
                        break;
                    case { Estado: "Suspendida" }:
                        return BadRequest("La reunión ya está suspendida");
                        break;
                    case { Estado: "Completada" }: 
                        return BadRequest("La reunión ya está completada");
                        break;

                }
                reunion.Estado = "Completada";
                await _context.SaveChangesAsync();
                return Ok("Reunión completada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar la reunión");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }
        [HttpPut("SuspenderReunion/{id}")]
        public async Task<IActionResult> SuspenderReunion(int id)
        {
            try
            {
                var reunion = await _context.Reuniones.FindAsync(id);

                switch (reunion)
                {
                    case null:
                        return NotFound("Reunión no encontrada");
                        break;
                    case { Estado: "Suspendida" }:
                        return BadRequest("La reunión ya está suspendida");
                        break;
                    case { Estado: "Completada" }:
                        return BadRequest("La reunión ya está completada");
                        break;
                }

                reunion.Estado = "Suspendida";
                await _context.SaveChangesAsync();

                var emailRequest = new EmailRequest
                {
                    To = reunion.Correo!,
                    Subject = "Suspensión de reunión programada",
                    Message = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>
<body style='font-family: Arial, sans-serif; padding: 20px;'>
    <p>Estimado/a:</p>

    <p>
        Le informamos que la reunión programada para el día 
        <strong>{reunion.Fecha:dd/MM/yyyy}</strong> a las 
        <strong>{reunion.Fecha:HH:mm}</strong> ha sido <strong>suspendida</strong>.
    </p>

    <p>
        En caso de reprogramarse, nos comunicaremos oportunamente.
    </p>

    <p>
        Saludos cordiales,<br/>
        <strong>Sistema MPN</strong>
    </p>
</body>
</html>"
                };

                await _emailService.SendEmailAsync(emailRequest);

                // Notificar al líder si la reunión venía de una petición
                if (reunion.IdPeticion.HasValue && reunion.IdPeticion.Value > 0)
                {
var peticion = await _context.Peticiones
                        .Include(p => p.IdLiderNavigation)
                            .ThenInclude(l => l.Usuario)
                        .FirstOrDefaultAsync(p => p.IdPeticiones == reunion.IdPeticion.Value);

if (peticion != null && peticion.IdLiderNavigation?.Usuario?.Correo != null)
                    {
                        var emailLider = new EmailRequest
                        {
                            To = peticion.IdLiderNavigation.Usuario.Correo,
                            Subject = "Suspensión de reunión solicitada",
                            Message = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>
<body style='font-family: Arial, sans-serif; padding: 20px;'>
    <p>Estimado líder:</p>

    <p>
        Le informamos que la reunión programada para el día 
        <strong>{reunion.Fecha:dd/MM/yyyy}</strong> a las 
        <strong>{reunion.Fecha:HH:mm}</strong> ha sido <strong>suspendida</strong>.
    </p>

    <p>
        En caso de reprogramarse, nos comunicaremos oportunamente.
    </p>

    <p>
        Saludos cordiales,<br/>
        <strong>Sistema MPN</strong>
    </p>
</body>
</html>"
                        };

                        try
                        {
                            await _emailService.SendEmailAsync(emailLider);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error al enviar correo de suspensión al líder");
                        }
                    }
                }

                return Ok("Reunión suspendida exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al suspender la reunión");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }
    }
}
