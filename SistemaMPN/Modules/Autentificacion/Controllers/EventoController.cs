using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class EventoController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<EventoController> _logger;
        private readonly IEmailService _emailService;

        public EventoController(DataContext context, ILogger<EventoController> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpGet("GetEventos")]
        public async Task<ActionResult<EventoDTO>> GetEventos()
        {
            var rolesUsuario = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            try
            {
                var listaEventos = await _context.Eventos
                    .Where(e => e.Fecha >= DateTime.Today && (
                                !e.IdRols.Any()
                                || rolesUsuario.Contains("admin")
                                || rolesUsuario.Contains("auditor")
                                || e.IdRols.Any(r => rolesUsuario.Contains(r.Nombre.ToLower()))
                    ))
                    .Select(e => new EventoDTO
                    {
                        IdEvento = e.IdEvento,
                        Titulo = e.Titulo,
                        Lugar = e.Lugar,
                        Calle = e.Calle,
                        Altura = e.Altura,
                        Color = e.Color,
                        Fecha = e.Fecha,
                        HoraInicio = e.HoraInicio,
                        HoraFin = e.HoraFin,
                        Duracion_completa = e.Duracion_completa,
                        Roles = e.IdRols.Select(r => new RolDTO { id = r.IdRol, nombre = r.Nombre }).ToList()
                    })
                    .ToListAsync();

                return Ok(listaEventos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los eventos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("CrearEvento")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<ActionResult<int>> CrearEvento(EventoDTO evento)
        {
            try
            {
                var eventoNuevo = new Evento
                {
                    Titulo = evento.Titulo,
                    Lugar = evento.Lugar,
                    Calle = evento.Calle,
                    Altura = evento.Altura,
                    Color = evento.Color,
                    Fecha = evento.Fecha,
                    HoraInicio = evento.HoraInicio,
                    HoraFin = evento.HoraFin,
                    Duracion_completa = evento.Duracion_completa
                };

                if (evento.Roles != null && evento.Roles.Any())
                {
                    var rolsIds = evento.Roles.Select(s => s.id).ToList();
                    var roles = await _context.Roles
                        .Where(r => rolsIds.Contains(r.IdRol))
                        .ToListAsync();

                    foreach (var rol in roles)
                    {
                        rol.IdEventos.Add(eventoNuevo);
                    }
                }

                _context.Eventos.Add(eventoNuevo);
                await _context.SaveChangesAsync();

                await NotificarCreacionEventoAsync(eventoNuevo, evento.Roles);

                return Ok(eventoNuevo.IdEvento);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crear el evento");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("ModificarEvento")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<ActionResult<string>> ModificarEvento(EventoDTO evento)
        {
            var eventoModificado = await _context.Eventos.Include(r => r.IdRols).FirstOrDefaultAsync(e => e.IdEvento == evento.IdEvento);
            if (eventoModificado == null)
            {
                return NotFound("Envento no encontrado");
            }

            eventoModificado.Titulo = evento.Titulo;
            eventoModificado.Lugar = evento.Lugar;
            eventoModificado.Calle = evento.Calle;
            eventoModificado.Altura = evento.Altura;
            eventoModificado.Fecha = evento.Fecha;
            eventoModificado.HoraInicio = evento.HoraInicio;
            eventoModificado.HoraFin = evento.HoraFin;
            eventoModificado.Color = evento.Color;
            eventoModificado.Duracion_completa = evento.Duracion_completa;

            eventoModificado.IdRols.Clear();
            if (evento.Roles != null && evento.Roles.Any())
            {
                var rolsIds = evento.Roles.Select(s => s.id).ToList();
                var nuevosRoles = await _context.Roles
                    .Where(r => rolsIds.Contains(r.IdRol))
                    .ToListAsync();

                foreach (var rol in nuevosRoles)
                {
                    eventoModificado.IdRols.Add(rol);
                }
            }

            await _context.SaveChangesAsync();

            await NotificarModificacionEventoAsync(eventoModificado, evento);

            return Ok("Evento modificado exitosamente");
        }

        [HttpDelete("EliminarEvento/{idEvento}")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<ActionResult<string>> EliminarEvento(int idEvento)
        {
            var evento = await _context.Eventos.Include(r => r.IdRols).FirstOrDefaultAsync(e => e.IdEvento == idEvento);

            if (evento == null)
            {
                return NotFound("Evento no encontrado");
            }

            await NotificarEliminacionEventoAsync(evento);

            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();

            return Ok("Evento eliminado exitosamente");
        }

        private async Task NotificarCreacionEventoAsync(Evento evento, List<RolDTO>? roles)
        {
            var destinatarios = await ObtenerCorreosPorRolesAsync(evento.IdRols.ToList(), roles);
            if (!destinatarios.Any()) return;

            var fechaFormateada = evento.Fecha?.ToString("dd/MM/yyyy") ?? "Sin fecha";
            var horaInicio = evento.HoraInicio.HasValue ? evento.HoraInicio.Value.ToString(@"hh\:mm") : "Sin hora";
            var horaFin = evento.HoraFin.HasValue ? evento.HoraFin.Value.ToString(@"hh\:mm") : "";
            var duracion = evento.Duracion_completa ? "Todo el día" : $"{horaInicio} - {horaFin}";

            var nombresRoles = evento.IdRols.Select(r => r.Nombre).ToList();
            var rolesTexto = string.Join(", ", nombresRoles);

            var mensaje = $@"
                <h2>Nuevo Evento Creado</h2>
                <p>Se ha creado un nuevo evento en el sistema.</p>
                <h3>Detalles del Evento:</h3>
                <ul>
                    <li><strong>Título:</strong> {evento.Titulo}</li>
                    <li><strong>Fecha:</strong> {fechaFormateada}</li>
                    <li><strong>Horario:</strong> {duracion}</li>
                    <li><strong>Lugar:</strong> {evento.Lugar}</li>
                    <li><strong>Dirección:</strong> {evento.Calle} {evento.Altura}</li>
                    <li><strong>Roles involucrados:</strong> {rolesTexto}</li>
                </ul>
                <p>Por favor, tome nota de esta información.</p>
            ";

            var request = new BulkEmailRequest
            {
                Recipients = destinatarios,
                Subject = $"Nuevo Evento: {evento.Titulo}",
                Message = mensaje
            };

            await _emailService.SendBulkEmailAsync(request);
        }

        private async Task NotificarModificacionEventoAsync(Evento eventoAnterior, EventoDTO eventoActualizado)
        {
            var destinatariosRoles = await ObtenerCorreosPorRolesAsync(
                await _context.Roles.Where(r => eventoActualizado.Roles!.Select(x => x.id).Contains(r.IdRol)).ToListAsync(),
                eventoActualizado.Roles);

            var destinatariosAsistentes = await ObtenerCorreosAsistentesAsync(eventoAnterior.IdEvento);
            var destinatarios = destinatariosRoles.Union(destinatariosAsistentes).Distinct().ToList();

            if (!destinatarios.Any()) return;

            var cambios = new List<string>();

            if (eventoAnterior.Titulo != eventoActualizado.Titulo)
                cambios.Add($"<li><strong>Título:</strong> {eventoAnterior.Titulo} → {eventoActualizado.Titulo}</li>");
            if (eventoAnterior.Fecha != eventoActualizado.Fecha)
                cambios.Add($"<li><strong>Fecha:</strong> {eventoAnterior.Fecha:dd/MM/yyyy} → {eventoActualizado.Fecha:dd/MM/yyyy}</li>");
            if (eventoAnterior.HoraInicio != eventoActualizado.HoraInicio)
                cambios.Add($"<li><strong>Hora de inicio:</strong> {eventoAnterior.HoraInicio:hh\\:mm} → {eventoActualizado.HoraInicio:hh\\:mm}</li>");
            if (eventoAnterior.HoraFin != eventoActualizado.HoraFin)
                cambios.Add($"<li><strong>Hora de fin:</strong> {eventoAnterior.HoraFin:hh\\:mm} → {eventoActualizado.HoraFin:hh\\:mm}</li>");
            if (eventoAnterior.Lugar != eventoActualizado.Lugar)
                cambios.Add($"<li><strong>Lugar:</strong> {eventoAnterior.Lugar} → {eventoActualizado.Lugar}</li>");
            if (eventoAnterior.Calle != eventoActualizado.Calle)
                cambios.Add($"<li><strong>Calle:</strong> {eventoAnterior.Calle} → {eventoActualizado.Calle}</li>");
            if (eventoAnterior.Altura != eventoActualizado.Altura)
                cambios.Add($"<li><strong>Altura:</strong> {eventoAnterior.Altura} → {eventoActualizado.Altura}</li>");
            if (eventoAnterior.Duracion_completa != eventoActualizado.Duracion_completa)
                cambios.Add($"<li><strong>Duración completa:</strong> {(eventoAnterior.Duracion_completa ? "Sí" : "No")} → {(eventoActualizado.Duracion_completa ? "Sí" : "No")}</li>");

            if (cambios.Any())
            {
                var cambiosHtml = string.Join("", cambios);
                var mensaje = $@"
                    <h2>Evento Modificado</h2>
                    <p>El evento <strong>{eventoActualizado.Titulo}</strong> ha sido modificado.</p>
                    <h3>Cambios realizados:</h3>
                    <ul>
                        {cambiosHtml}
                    </ul>
                    <p>Por favor, tome nota de los cambios.</p>
                ";

                var request = new BulkEmailRequest
                {
                    Recipients = destinatarios,
                    Subject = $"Evento Modificado: {eventoActualizado.Titulo}",
                    Message = mensaje
                };

                await _emailService.SendBulkEmailAsync(request);
            }
        }

        private async Task NotificarEliminacionEventoAsync(Evento evento)
        {
            var destinatariosRoles = await ObtenerCorreosPorRolesAsync(evento.IdRols.ToList(), null);
            var destinatariosAsistentes = await ObtenerCorreosAsistentesAsync(evento.IdEvento);
            var destinatarios = destinatariosRoles.Union(destinatariosAsistentes).Distinct().ToList();

            if (!destinatarios.Any()) return;

            var mensaje = $@"
                <h2>Evento Cancelado</h2>
                <p>El evento <strong>{evento.Titulo}</strong> ha sido cancelado y eliminado del sistema.</p>
                <p>Detalles del evento cancelado:</p>
                <ul>
                    <li><strong>Fecha:</strong> {evento.Fecha:dd/MM/yyyy}</li>
                    <li><strong>Lugar:</strong> {evento.Lugar}</li>
                </ul>
                <p>Disculpe las molestias.</p>
            ";

            var request = new BulkEmailRequest
            {
                Recipients = destinatarios,
                Subject = $"Evento Cancelado: {evento.Titulo}",
                Message = mensaje
            };

            await _emailService.SendBulkEmailAsync(request);
        }

        private async Task<List<string>> ObtenerCorreosAsistentesAsync(int eventoId)
        {
            var correosVisitantes = await _context.Asistencias
                .Where(a => a.EventoId == eventoId && a.EmailVisitante != null)
                .Select(a => a.EmailVisitante)
                .Distinct()
                .ToListAsync();

            var correosMiembros = await _context.Asistencias
                .Where(a => a.EventoId == eventoId && a.MiembroId != null)
                .Include(a => a.Miembro)
                .ThenInclude(m => m!.Usuario)
                .Where(a => a.Miembro != null && a.Miembro.Usuario != null && a.Miembro.Usuario.Correo != null)
                .Select(a => a.Miembro!.Usuario!.Correo)
                .Distinct()
                .ToListAsync();

            var todosCorreos = correosVisitantes
                .Concat(correosMiembros)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            return todosCorreos!;
        }

        private async Task<List<string>> ObtenerCorreosPorRolesAsync(List<Rol> rolesDelEvento, List<RolDTO>? rolesDto)
        {
            var rolIds = rolesDto?.Select(r => r.id).ToList() ?? rolesDelEvento.Select(r => r.IdRol).ToList();

            var destinatarios = await _context.Usuarios
                .Include(u => u.IdRols)
                .Where(u => u.Correo != null && u.IdRols.Any(r => rolIds.Contains(r.IdRol)))
                .Select(u => u.Correo)
                .ToListAsync();

            return destinatarios!;
        }
    }
}
