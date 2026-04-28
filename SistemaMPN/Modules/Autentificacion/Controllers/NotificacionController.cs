using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Autentificacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,gestor de miembros,tesorero,protesorero,consultor,auditor")]
    public class NotificacionController : ControllerBase
    {
        private readonly DataContext _context;
        public NotificacionController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("GetNotificaciones")]
        public async Task<ActionResult<List<NotificacionDto>>> GetNotificaciones()
        {
            try
            {
                int? usuarioId = _context.Usuarios
                                         .Where(u => u.UserName.Equals(User.FindFirst(ClaimTypes.Name)!.Value))
                                         .Select(u => u.IdUsuarios).First();

                if (usuarioId == null) return StatusCode(404, "Usuario no encontrado.");

                var lista = await _context.Notificaciones
                    .Where(n => n.Fecha >= DateTime.UtcNow.AddDays(-7))
                    .Where(n => n.NotificacionUsuarios.Any(nu => nu.IdUsuario == usuarioId))
                    .OrderByDescending(n => n.Fecha)
                    .Take(5)
                    .Include(n => n.NotificacionUsuarios)
                    .Select(n => new NotificacionDto
                    {
                        Id = n.IdNotificaciones,
                        Mensaje = n.Mensaje ?? string.Empty,
                        Fecha = n.Fecha ?? DateTime.UtcNow,
                        Tipo = n.Tipo ?? string.Empty,
                        Leida = n.NotificacionUsuarios
                                 .Where(nu => nu.IdUsuario == usuarioId)
                                 .Select(nu => nu.Leida)
                                 .FirstOrDefault()
                    })
                    .ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPatch("LeerNotificacion")]
        public async Task<ActionResult> LeerNotificacion([FromBody] List<int> ids)
        {

            int? usuarioId = _context.Usuarios
                                          .Where(u => u.UserName.Equals(User.FindFirst(ClaimTypes.Name)!.Value))
                                          .Select(u => u.IdUsuarios).First();

            if (usuarioId == null) 
                return BadRequest("Usuario no encontrado");
            
            var notificacionesUsuario = await _context.NotificacionUsuario
                .Where(nu => ids.Contains(nu.IdNotificacion) && nu.IdUsuario == usuarioId)
                .ToListAsync();

            notificacionesUsuario.ForEach(nu => nu.Leida = true);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
