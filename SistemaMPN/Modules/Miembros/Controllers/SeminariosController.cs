using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Modules.Miembros.Services;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Miembros.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SeminariosController : ControllerBase
    {
        private readonly ISeminarioService _seminarioService;
        private readonly DataContext _context;

        public SeminariosController(ISeminarioService seminarioService, DataContext context)
        {
            _seminarioService = seminarioService;
            _context = context;
        }

        [HttpGet("GetSeminarios")]
        [Authorize(Roles = "admin,gestor de miembros,auditor")]
        public async Task<ActionResult<List<SeminarioDTO>>> GetSeminarios()
        {
            try
            {
                var lista = await _context.Seminarios
                    .Select(s => new SeminarioDTO { id = s.IdSeminario, nombre = s.Nombre, anio_comienzo = s.AnioComienzo, activo = s.Activo ?? false })
                    .ToListAsync();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [Authorize(Roles = "admin,gestor de miembros")]
        [HttpPost("CrearSeminario")]
        public async Task<ActionResult<string>> CrearSeminario(SeminarioDTO seminarioDTO)
        {
            try
            {
                var seminario = new Seminario();
                seminario.Nombre = seminarioDTO.nombre;
                seminario.AnioComienzo = seminarioDTO.anio_comienzo;
                seminario.Activo = seminarioDTO.activo;
                _context.Add(seminario);
                await _context.SaveChangesAsync();

                return Ok("Seminario registrado exitosamente");
            }
            catch 
            {
                return StatusCode(500, "Error interno al registrar usuario");
            }
        }

        [Authorize(Roles = "admin,gestor de miembros,auditor")]
        [HttpGet("{id}")]
        public async Task<ActionResult<SeminarioDTO>> GetSeminarioPorId(int id)
        {

            var seminario = await _context.Seminarios.FindAsync(id);
            if (seminario == null)
                return NotFound();

            return Ok(seminario);
        }

        [HttpPut("{id}/estado")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<ActionResult<bool>> ActualizarEstadoActivo(int id, [FromBody] bool nuevoEstado)
        {
            try
            {
                var seminario = await _context.Seminarios.FindAsync(id);
                if (seminario == null)
                    return false;

                seminario.Activo = nuevoEstado;
                _context.Seminarios.Update(seminario);

                var resultado = await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch
            {
                return BadRequest(false);
            }
           
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<IActionResult> ActualizarSeminario(int id, [FromBody] SeminarioDTO seminarioActualizado)
        {
            var existente = await _context.Seminarios.FindAsync(id);
            if (existente == null)
                return NotFound();

            existente.Nombre = seminarioActualizado.nombre;
            existente.AnioComienzo = seminarioActualizado.anio_comienzo;
            existente.Activo = seminarioActualizado.activo;

            _context.Seminarios.Update(existente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}