using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Modules.Autentificacion.Services;
using SistemaMPN.Modules.Miembros.Controllers;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Tesoreria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "protesorero,tesorero,admin,auditor")]
    public class TurnoController : ControllerBase
    {
        private readonly ILogger<TurnoController> _logger;
        private readonly DataContext _context;
        private readonly INotificacionService _notificacionService;
        public TurnoController(ILogger<TurnoController> logger, DataContext context, INotificacionService notificacionService)
        {
            _logger = logger;
            _context = context;
            _notificacionService = notificacionService;
        }

        [HttpGet("GetIdTesoreroActual")]
        public async Task<ActionResult<int>> GetIdTesoreroActual()
        {
            try
            {
                int? usuarioId = _context.Usuarios
                                         .Where(u => u.UserName.Equals(User.FindFirst(ClaimTypes.Name)!.Value))
                                         .Select(u => u.IdMiembros).First();

                if (usuarioId == null) return StatusCode(404, "Usuario no encontrado.");

                var tesorero = await _context.Tesoreros
                    .Where(t => t.IdMiembros == usuarioId)
                    .FirstOrDefaultAsync();

                if (tesorero == null) return Ok(usuarioId);

                return Ok(tesorero.IdMiembros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el id del tesorero actual.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [HttpGet("GetTesorerosDisponibles")]
        public async Task<ActionResult<List<TesoreroDisponibleDTO>>> GetTesorerosDisponibles()
        {
            try
            {
                var tesoreros = await _context.Tesoreros
                    .Include(t => t.IdMiembrosNavigation)
                    .Select(t => new TesoreroDisponibleDTO
                    {
                        IdTesorero = t.IdMiembrosNavigation.IdMiembros,
                        Nombre = t.IdMiembrosNavigation.Nombre + " " + t.IdMiembrosNavigation.Apellido
                    })
                    .ToListAsync();
                return Ok(tesoreros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los tesoreros.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [HttpGet("GetTurno/{idTurno}")]
        public async Task<ActionResult<TurnoDTO>> GetTurno(int idTurno)
        {
            try
            {
                var turno = await _context.Turnos
                    .Where(t => t.IdTurnos == idTurno)
                    .Select(t => new TurnoDTO
                    {
                        IdTurno = t.IdTurnos,
                        Fecha = t.Fecha,
                        HoraInicio = t.HoraInicio,
                        Color = t.Color,
                        IdTesorero = t.IdTesorero ?? 0
                    })
                    .FirstOrDefaultAsync();
                if (turno == null)
                {
                    return NotFound("Turno no encontrado.");
                }
                return Ok(turno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el turno.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [HttpGet("GetTurnos")]
        public async Task<ActionResult<List<TurnoDTO>>> GetTurnos()
        {
            try
            {
                int? usuarioId = _context.Usuarios
                                         .Where(u => u.UserName.Equals(User.FindFirst(ClaimTypes.Name)!.Value))
                                         .Select(u => u.IdMiembros).First();

                if (usuarioId == null) return StatusCode(404, "Usuario no encontrado.");

                bool esTesorero = _context.Tesoreros.Any(t => t.IdMiembros == usuarioId && !t.IsPro);

                var query = _context.Turnos.Where(t => t.Fecha >= DateTime.Today);

                if (esTesorero)
                {
                    query = query.Where(t => t.IdTesorero == usuarioId);
                }

                var turnos = await query
                    .Select(t => new TurnoDTO
                    {
                        IdTurno = t.IdTurnos,
                        Fecha = t.Fecha,
                        HoraInicio = t.HoraInicio,
                        Color = t.Color,
                        IdTesorero = t.IdTesorero ?? 0
                    })
                    .ToListAsync();

                return Ok(turnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los turnos.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [HttpGet("GetPropuestas")]
        public async Task<ActionResult<List<PropuestaDeCambioDTO>>> GetPropuestas()
        {
            try
            {
                int? usuarioId = _context.Usuarios
                    .Where(u => u.UserName == User.FindFirst(ClaimTypes.Name)!.Value)
                    .Select(u => u.IdMiembros)
                    .FirstOrDefault();

                if (usuarioId == null)
                    return NotFound("Usuario no encontrado.");

                bool esTesorero = _context.Tesoreros.Any(t =>
                    t.IdMiembros == usuarioId && !t.IsPro);

                var query = _context.PropuestaCambioTurnos.AsQueryable();

                if (esTesorero)
                {
                    query = query.Where(p => p.Estado == "Pendiente" && p.IdReceptor == usuarioId).Take(5);
                }

                var propuestas = await query
                    .OrderByDescending(p => p.FechaSolicitud)
                    .Select(p => new PropuestaDeCambioDTO
                    {
                        IdPropuestaDeCambio = p.IdPropuestaCambioTurno,
                        Estado = p.Estado,
                        FechaSolicitud = p.FechaSolicitud,
                        IdReceptor = p.IdReceptor ?? 0,
                        TurnoActual = p.IdTurno ?? 0
                    })
                    .ToListAsync();

                return Ok(propuestas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las propuestas.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [HttpPost("PedirCambio")]
        public async Task<ActionResult<string>> PedirCambio(PropuestaDeCambioDTO propuestaDTO)
        {
            try
            {
                var propuestaExistente = await _context.PropuestaCambioTurnos
                                    .Where(p => p.IdTurno == propuestaDTO.TurnoActual && p.Estado.Equals("Pendiente"))
                                    .AnyAsync();

                if (propuestaExistente) return BadRequest("Ya se pidio cambio para el turno");

                var queryTurno = _context.Turnos.AsQueryable();

                var turnoExistente = await queryTurno
                                    .Where(t => t.IdTurnos == propuestaDTO.TurnoActual)
                                    .FirstOrDefaultAsync();
                if (turnoExistente == null) return BadRequest("El turno no existe");

                var turnoSuperpuesto = await queryTurno
                                    .Where(t => t.IdTesorero == propuestaDTO.IdReceptor
                                                && t.Fecha == turnoExistente.Fecha
                                                && t.HoraInicio >= turnoExistente.HoraInicio)
                                    .AnyAsync();

                if (turnoSuperpuesto) return BadRequest("El tesorero receptor tiene un turno que se superpone con el turno actual.");



                var nuevaPropuesta = new PropuestaCambioTurno
                {
                    Estado = "Pendiente",
                    FechaSolicitud = DateTime.Now,
                    IdReceptor = propuestaDTO.IdReceptor,
                    IdTurno = propuestaDTO.TurnoActual
                };

                _context.PropuestaCambioTurnos.Add(nuevaPropuesta);
                await _context.SaveChangesAsync();

                await _notificacionService.EnviarNotificacionPropuesta(propuestaDTO);

                return Ok("Propuesta guardada correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los turnos.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [HttpPut("ActualizarPropuesta/{idPropuesta}/{nuevoEstado}")]
        public async Task<ActionResult<string>> ActualizarPropuesta(int idPropuesta, string nuevoEstado)
        {
            try
            {
                var propuesta = await _context.PropuestaCambioTurnos
                                    .Where(p => p.IdPropuestaCambioTurno == idPropuesta)
                                    .FirstAsync();

                if (propuesta == null) return BadRequest("La propuesta de cambio no existe");

                nuevoEstado = nuevoEstado.Trim();
                if (!nuevoEstado.Equals("Aceptada") && !nuevoEstado.Equals("Rechazada")) return BadRequest("Estado de propuesta no valido");

                if (nuevoEstado.Equals("Aceptada"))
                {
                    var turno = await _context.Turnos
                                        .Where(t => t.IdTurnos == propuesta.IdTurno)
                                        .FirstAsync();
                    turno.IdTesorero = propuesta.IdReceptor;
                    _context.Turnos.Update(turno);
                }

                propuesta.Estado = nuevoEstado;
                propuesta.FechaRespuesta = DateTime.UtcNow;

                _context.PropuestaCambioTurnos.Update(propuesta);
                await _context.SaveChangesAsync();

                return Ok("Propuesta guardada correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los turnos.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [HttpPost("AsignarTurnos")]
        public async Task<ActionResult<string>> AsignarTurnos(List<TurnoDTO> turnosDTO)
        {
            try
            {
                foreach (var turnoDTO in turnosDTO)
                {
                    var nuevoTurno = new Turno
                    {
                        Fecha = turnoDTO.Fecha,
                        HoraInicio = turnoDTO.HoraInicio,
                        Color = turnoDTO.Color,
                        IdTesorero = turnoDTO.IdTesorero
                    };
                    _context.Turnos.Add(nuevoTurno);
                }
                await _context.SaveChangesAsync();
                return Ok("Turnos asignados correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar los turnos.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }

        [HttpPatch("ActualizarTurno/{idTurno}")]
        public async Task<ActionResult<string>> ActualizarTurno(int idTurno, TurnoDTO turnoDTO)
        {
            try
            {
                var turno = await _context.Turnos
                                    .Where(t => t.IdTurnos == idTurno)
                                    .FirstAsync();

                if (turno == null) return BadRequest("El turno no existe");
                if (turno.IdTesorero == null) return BadRequest("El turno no tiene un tesorero asignado");

                turno.Fecha = turnoDTO.Fecha;
                turno.HoraInicio = turnoDTO.HoraInicio;
                turno.Color = turnoDTO.Color;
                turno.IdTesorero = turnoDTO.IdTesorero;

                _context.Turnos.Update(turno);

                await _context.SaveChangesAsync();
                return Ok("Turno actualizado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el turno.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }
    }
}
