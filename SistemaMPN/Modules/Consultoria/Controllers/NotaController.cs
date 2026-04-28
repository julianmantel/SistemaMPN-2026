using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;
using static SistemaMPN.Client.Modules.Consultoria.Dialogs.DialogAgregarNota;

namespace SistemaMPN.Modules.Consultoria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotaController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<NotaController> _logger;
        public NotaController(DataContext context, ILogger<NotaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("GetNotasReunion/{id_reunion}")]
        [Authorize(Roles = "admin,consultor,auditor")]
        public async Task<IActionResult> GetNotasReunion(int id_reunion)
        {
            try
            {
                var notas = await _context.Notas
                    .Where(n => n.IdReunion == id_reunion).Select(n => new NotaDTO
                    {
                        IdNota = n.IdNota,
                        Comentarios = n.Comentarios,
                        IdReunion = n.IdReunion
                    }).ToListAsync();
                return Ok(notas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las notas de la reunion");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }

        [HttpPost("AgregarNota")]
        [Authorize(Roles = "admin,consultor")]
        public async Task<IActionResult> AgregarNota(NotaDTO notaDto)
        {
            try
            {
                var nota = new Nota
                {
                    Comentarios = notaDto.Comentarios,
                    IdReunion = notaDto.IdReunion,
                };

                _context.Notas.Add(nota);
                await _context.SaveChangesAsync();
                return Ok(nota);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar la nota");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }

        [HttpDelete("EliminarNota/{id_nota}")]
        [Authorize(Roles = "admin,consultor")]
        public async Task<IActionResult> EliminarNota(int id_nota)
        {
            try
            {
                var nota = await _context.Notas.FindAsync(id_nota);
                if (nota == null)
                {
                    return NotFound();
                }
                _context.Notas.Remove(nota);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la nota");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno del servidor");
            }
        }
    }
}
