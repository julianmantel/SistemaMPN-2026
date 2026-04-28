using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Controllers;
using SistemaMPN.Data;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Miembros.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin, gestor de miembros, lider,auditor")]
    public class LocalizacionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<UsuarioController> _logger;

        public LocalizacionController(DataContext context, ILogger<UsuarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("GetInfoGrupos")]
        public async Task<ActionResult<List<GrupoDTO>>> GetInfoGrupos()
        {
            try
            {
                var grupos = await _context.Grupos
                    .Where(g => !g.IdLocalizaciones.HasValue)
                    .Select(g => new GrupoDTO
                    {
                        id_grupo = g.IdGrupos,
                        nombre = g.Nombre ?? string.Empty
                    })
                    .ToListAsync();

                return Ok(grupos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los grupos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetLocalizaciones")]
        public async Task<ActionResult<List<LocalizacionDTO>>> GetLocalizaciones()
        {
            try
            {
                var localizaciones = await _context.Localizaciones
                    .Select(l => new LocalizacionDTO
                    {
                        IdLocalizacion = l.IdLocalizaciones,
                        Tipo = l.Tipo,
                        Ubicacion = l.Ubicacion,
                        Grupo = l.Grupos.Select(g => new GrupoDTO
                        {
                            id_grupo = g.IdGrupos,
                            nombre = g.Nombre
                        }).FirstOrDefault()!,
                        Direccion = l.Direccion
                    }).ToListAsync();

                return Ok(localizaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las localizaciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("CrearLocalizacion")]
        public async Task<ActionResult<string>> CrearLocalizacion(LocalizacionDTO localizacionDTO)
        {
            try
            {

                var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.IdGrupos == localizacionDTO.Grupo.id_grupo);

                if (grupo == null)
                {
                    return NotFound("El grupo no existe");
                }

                Localizacion localizacion = new Localizacion
                {
                    Tipo = localizacionDTO.Tipo,
                    Direccion = localizacionDTO.Direccion,
                    Ubicacion = localizacionDTO.Ubicacion
                };

                localizacion.Grupos.Add(grupo);

                _context.Localizaciones.Add(localizacion);
                await _context.SaveChangesAsync();

                return Ok("Localizacion añadida correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear localizacion");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPatch("ModificarLocalizacion")]
        public async Task<ActionResult<string>> ModificarLocalizacion(LocalizacionDTO localizacionDTO)
        {
            var localizacion = await _context.Localizaciones
                .Include(l => l.Grupos)
                .FirstAsync(l => l.IdLocalizaciones == localizacionDTO.IdLocalizacion);
            if(localizacion == null)
            {
                return NotFound("La localizacion no existe");
            }

            localizacion.Tipo = localizacionDTO.Tipo;
            localizacion.Direccion = localizacionDTO.Direccion;

            var grupoActual = localizacion.Grupos.FirstOrDefault();
            var grupoNuevo = await _context.Grupos
                .FirstOrDefaultAsync(g => g.Nombre == localizacionDTO.Grupo.nombre);

            if (grupoNuevo == null)
                return NotFound("El grupo destino no existe");

            if (grupoActual != null && grupoActual.IdGrupos != grupoNuevo.IdGrupos)
            {
                grupoActual.IdLocalizaciones = null;
                grupoNuevo.IdLocalizaciones = localizacionDTO.IdLocalizacion;
            }

            await _context.SaveChangesAsync();

            return Ok("Localizacion modificada correctamente");
        }

        [HttpDelete("EliminarLocalizacion/{id}")]
        public async Task<ActionResult<string>> EliminarLocalizacion(int id)
        {
            var localizacion = await _context.Localizaciones
                .Include(l => l.Grupos)
                .FirstAsync(l => l.IdLocalizaciones == id);

            if (localizacion == null)
            {
                return NotFound("La localizacion no existe");
            }

            _context.Localizaciones.Remove(localizacion);
            await _context.SaveChangesAsync();

            return Ok("Localizacion eliminada correctamente");
        }
    }
}
