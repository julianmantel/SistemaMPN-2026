using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Miembros.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin, gestor de miembros, lider,auditor")]
    public class GrupoController : ControllerBase
    {
        private readonly ILogger<GrupoController> _logger;
        private readonly DataContext _context;
        public GrupoController(ILogger<GrupoController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("AgregarMiembroAlGrupo/{idGrupo}/{idMiembro}")]
        public async Task<IActionResult> AgregarMiembroAlGrupo(int idGrupo, int idMiembro, [FromBody] string ocupacion)
        {
            try
            {
                // Verificar que el grupo existe
                var grupo = await _context.Grupos.FindAsync(idGrupo);
                if (grupo == null)
                {
                    return NotFound("El grupo no existe");
                }
                // Verificar que el miembro existe y está activo
                var miembro = await _context.Miembros.FindAsync(idMiembro);
                if (miembro == null || miembro.FechaHasta != null)
                {
                    return BadRequest("El miembro no existe o no está activo");
                }
                // Verificar que el miembro no esté ya en el grupo
                var yaEnGrupo = await _context.PerteneceGrupos
                    .AnyAsync(pg => pg.IdGrupos == idGrupo && pg.IdMiembros == idMiembro);
                if (yaEnGrupo)
                {
                    return BadRequest("El miembro ya pertenece a este grupo");
                }
                // Verificar que el grupo no haya alcanzado su capacidad máxima
                if (grupo.CantidadMiembros >= grupo.MaxCantMiembros)
                {
                    return BadRequest("El grupo ha alcanzado su capacidad máxima de miembros");
                }
                // Agregar al miembro al grupo
                var perteneceGrupo = new PerteneceGrupo
                {
                    IdGrupos = idGrupo,
                    IdMiembros = idMiembro,
                    FechaDesde = DateOnly.FromDateTime(DateTime.Now),
                    Ocupacion = ocupacion
                };
                _context.PerteneceGrupos.Add(perteneceGrupo);
                // Incrementar la cantidad de miembros del grupo
                grupo.CantidadMiembros++;
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    mensaje = "Miembro agregado al grupo correctamente",
                    nuevaCantidadMiembros = grupo.CantidadMiembros
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("GetMiembrosSinGrupo")]
        public async Task<ActionResult<List<InfoMiembroGrupoDTO>>> GetMiembrosSinGrupo()
        {
            try
            {
                var miembrosSinGrupo = await _context.Miembros
                    .Where(m => m.FechaHasta == null && !_context.PerteneceGrupos.Any(pg => pg.IdMiembros == m.IdMiembros))
                    .Select(m => new InfoMiembroGrupoDTO
                    {
                        id_miembro = m.IdMiembros,
                        nombre_completo = (m.Nombre ?? string.Empty) + " " + (m.Apellido ?? string.Empty),
                        dni = m.Dni,
                        ocupacion = "Sin grupo",
                        fecha_desde = null
                    })
                    .OrderBy(m => m.nombre_completo)
                    .ToListAsync();
                return Ok(miembrosSinGrupo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completo: {ex}");
                return BadRequest(new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpGet("GetMiembrosParaLiderazgo")]
        public async Task<ActionResult<List<InfoMiembroGrupoDTO>>> GetMiembrosParaLiderazgo()
        {
            try
            {
                // Primero obtener todos los miembros activos con usuario
                var miembrosConUsuario = await _context.Miembros
                    .Where(m => m.FechaHasta == null && m.Usuario != null)
                    .Select(m => new
                    {
                        IdMiembro = m.IdMiembros,
                        Nombre = m.Nombre,
                        Apellido = m.Apellido,
                        Dni = m.Dni,
                        IdUsuario = m.Usuario.IdUsuarios
                    })
                    .ToListAsync();

                // Obtener usuarios con rol de líder
                var usuariosConRolLider = await _context.Usuarios
                    .Where(u => u.IdRols.Any(r => r.Nombre.ToLower() == "lider"))
                    .Select(u => u.IdUsuarios)
                    .ToListAsync();

                // Obtener miembros que ya son líderes activos
                var lideresActivos = await _context.Lideres
                    .Select(l => l.IdMiembros)
                    .ToListAsync();

                // Filtrar en memoria
                var miembrosDisponibles = miembrosConUsuario
                    .Where(m => usuariosConRolLider.Contains(m.IdUsuario) &&
                               !lideresActivos.Contains(m.IdMiembro))
                    .Select(m => new InfoMiembroGrupoDTO
                    {
                        id_miembro = m.IdMiembro,
                        nombre_completo = $"{m.Nombre} {m.Apellido}",
                        dni = m.Dni,
                        ocupacion = "Líder",
                        fecha_desde = null
                    })
                    .OrderBy(m => m.nombre_completo)
                    .ToList();

                return Ok(miembrosDisponibles);
            }
            catch (Exception ex)
            {
                // Agregar más detalle del error para debug
                Console.WriteLine($"Error completo: {ex}");
                return BadRequest(new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPost("CrearGrupo")]
        public async Task<ActionResult<GrupoDTO>> CrearGrupo([FromBody] GrupoDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verificar que el miembro existe y está activo
                var lider = await _context.Miembros
                    .Include(m => m.Usuario)
                        .ThenInclude(u => u.IdRols)
                    .FirstOrDefaultAsync(m => m.IdMiembros == request.id_lider && m.FechaHasta == null);

                if (lider == null)
                {
                    return BadRequest("El líder seleccionado no existe o no está activo");
                }

                // Verificar que el miembro tenga un usuario vinculado
                if (lider.Usuario == null)
                {
                    return BadRequest("El miembro seleccionado no tiene un usuario del sistema vinculado");
                }

                // VERIFICAR QUE EL USUARIO YA TENGA EL ROL DE LÍDER
                var usuarioTieneRolLider = lider.Usuario.IdRols.Any(r => r.Nombre.ToLower() == "lider");

                if (!usuarioTieneRolLider)
                {
                    return BadRequest("El usuario seleccionado no tiene permisos de liderazgo en el sistema");
                }

                // Verificar que el miembro no sea ya líder de otro grupo
                var yaEsLider = await _context.Lideres
                    .AnyAsync(l => l.IdMiembros == request.id_lider);

                if (yaEsLider)
                {
                    return BadRequest("El miembro seleccionado ya es líder de otro grupo");
                }

                // Verificar que no exista un grupo con el mismo nombre
                var grupoExistente = await _context.Grupos
                    .AnyAsync(g => g.Nombre.ToLower() == request.nombre.ToLower().Trim());

                if (grupoExistente)
                {
                    return BadRequest("Ya existe un grupo con ese nombre");
                }

                // Crear el grupo
                var nuevoGrupo = new Grupo
                {
                    Nombre = request.nombre.Trim(),
                    CantidadMiembros = 1,
                    MaxCantMiembros = 12,
                    IdLocalizaciones = request.id_localizaciones
                };

                _context.Grupos.Add(nuevoGrupo);
                await _context.SaveChangesAsync();

                // Crear el registro de líder
                var nuevoLider = new Lider
                {
                    IdMiembros = lider.IdMiembros,
                    Tipo = "Grupo"
                };

                _context.Lideres.Add(nuevoLider);

                // Agregar al líder como miembro del grupo
                var perteneceGrupo = new PerteneceGrupo
                {
                    IdGrupos = nuevoGrupo.IdGrupos,
                    IdMiembros = lider.IdMiembros,
                    FechaDesde = DateOnly.FromDateTime(DateTime.Now),
                    Ocupacion = "Líder"
                };

                _context.PerteneceGrupos.Add(perteneceGrupo);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Crear el DTO de respuesta
                var grupoDTO = new GrupoDTO
                {
                    id_grupo = nuevoGrupo.IdGrupos,
                    nombre = nuevoGrupo.Nombre,
                    id_lider = request.id_lider,
                    lider_nombre = $"{lider.Nombre} {lider.Apellido}",
                    cantidad_miembros = nuevoGrupo.CantidadMiembros,
                    max_cant_miembros = nuevoGrupo.MaxCantMiembros,
                    id_localizaciones = nuevoGrupo.IdLocalizaciones
                };

                return Ok(grupoDTO);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("TransferirLiderazgo/{idGrupo}/{idNuevoLider}")]
        public async Task<IActionResult> TransferirLiderazgo(int idGrupo, int idNuevoLider)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verificar que el grupo existe
                var grupo = await _context.Grupos
                    .Include(g => g.PerteneceGrupos)
                    .FirstOrDefaultAsync(g => g.IdGrupos == idGrupo);
                if (grupo == null)
                {
                    return NotFound("El grupo no existe");
                }

                // Verificar que el nuevo líder existe y está activo
                var nuevoLider = await _context.Miembros
                    .Include(m => m.Usuario)
                        .ThenInclude(u => u.IdRols)
                    .FirstOrDefaultAsync(m => m.IdMiembros == idNuevoLider && m.FechaHasta == null);
                if (nuevoLider == null)
                {
                    return BadRequest("El nuevo líder no existe o no está activo");
                }
                if (nuevoLider.Usuario == null)
                {
                    return BadRequest("El nuevo líder no tiene un usuario del sistema vinculado");
                }

                var usuarioTieneRolLider = nuevoLider.Usuario.IdRols.Any(r => r.Nombre.ToLower() == "lider");
                if (!usuarioTieneRolLider)
                {
                    return BadRequest("El usuario seleccionado no tiene permisos de liderazgo en el sistema");
                }

                // Verificar que el nuevo líder no sea ya líder de otro grupo
                var yaEsLider = await _context.Lideres
                    .AnyAsync(l => l.IdMiembros == idNuevoLider);
                if (yaEsLider)
                {
                    return BadRequest("El miembro seleccionado ya es líder de otro grupo");
                }

                // Verificar que el nuevo líder pertenezca al grupo
                var perteneceAlGrupo = await _context.PerteneceGrupos
                    .AnyAsync(pg => pg.IdGrupos == idGrupo && pg.IdMiembros == idNuevoLider);
                if (!perteneceAlGrupo)
                {
                    return BadRequest("El nuevo líder debe ser miembro del grupo");
                }

                // Encontrar y actualizar el registro de líder actual
                var liderActual = await _context.PerteneceGrupos
                    .FirstOrDefaultAsync(pg => pg.IdGrupos == idGrupo && pg.Ocupacion == "Líder");

                if (liderActual != null)
                {
                    // Eliminar registro de líder actual
                    var lider = await _context.Lideres
                        .FirstOrDefaultAsync(l => l.IdMiembros == liderActual.IdMiembros);
                    if (lider != null)
                    {
                        _context.Lideres.Remove(lider);
                    }
                    // Cambiar ocupación del líder actual a "Miembro"
                    liderActual.Ocupacion = "Miembro";
                }

                // Crear nuevo registro de líder
                var nuevoRegistroLider = new Lider
                {
                    IdMiembros = idNuevoLider,
                    Tipo = "Grupo"
                };
                _context.Lideres.Add(nuevoRegistroLider);

                // Actualizar ocupación del nuevo líder
                var perteneceNuevoLider = await _context.PerteneceGrupos
                    .FirstOrDefaultAsync(pg => pg.IdGrupos == idGrupo && pg.IdMiembros == idNuevoLider);

                if (perteneceNuevoLider != null)
                {
                    perteneceNuevoLider.Ocupacion = "Líder";
                }
                else
                {
                    // Este caso no debería suceder por la validación anterior, pero por seguridad
                    var nuevoPertenece = new PerteneceGrupo
                    {
                        IdGrupos = idGrupo,
                        IdMiembros = idNuevoLider,
                        FechaDesde = DateOnly.FromDateTime(DateTime.Now),
                        Ocupacion = "Líder"
                    };
                    _context.PerteneceGrupos.Add(nuevoPertenece);
                    grupo.CantidadMiembros++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Liderazgo transferido correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("EliminarGrupo/{idGrupo}")]
        public async Task<IActionResult> EliminarGrupo(int idGrupo)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verificar que el grupo existe
                var grupo = await _context.Grupos
                    .Include(g => g.PerteneceGrupos)
                    .FirstOrDefaultAsync(g => g.IdGrupos == idGrupo);

                if (grupo == null)
                {
                    return NotFound("El grupo no existe");
                }

                // Encontrar y eliminar el registro de líder
                var liderGrupo = await _context.PerteneceGrupos
                    .FirstOrDefaultAsync(pg => pg.IdGrupos == idGrupo && pg.Ocupacion == "Líder");

                if (liderGrupo != null)
                {
                    var lider = await _context.Lideres
                        .FirstOrDefaultAsync(l => l.IdMiembros == liderGrupo.IdMiembros);

                    if (lider != null)
                    {
                        _context.Lideres.Remove(lider);
                    }
                }

                // Eliminar todas las relaciones miembro-grupo
                if (grupo.PerteneceGrupos.Any())
                {
                    _context.PerteneceGrupos.RemoveRange(grupo.PerteneceGrupos);
                }

                // Eliminar el grupo
                _context.Grupos.Remove(grupo);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Grupo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("GetGrupos")]
        public async Task<ActionResult<List<GrupoDTO>>> GetGrupos()
        {
            try
            {
                var grupos = await _context.Grupos
                    .Include(g => g.PerteneceGrupos)
                        .ThenInclude(pg => pg.IdMiembrosNavigation)
                    .Include(g => g.IdLocalizacionesNavigation)
                    .Select(g => new GrupoDTO
                    {
                        id_grupo = g.IdGrupos,
                        // Para obtener el líder, necesitamos buscar en la tabla Lider
                        id_lider = _context.Lideres
                            .Where(l => g.PerteneceGrupos.Any(pg => pg.IdMiembros == l.IdMiembros))
                            .Select(l => l.IdMiembros)
                            .FirstOrDefault(),
                        lider_nombre = _context.Lideres
                            .Where(l => g.PerteneceGrupos.Any(pg => pg.IdMiembros == l.IdMiembros))
                            .Select(l => l.IdMiembrosNavigation.Nombre + " " + l.IdMiembrosNavigation.Apellido)
                            .FirstOrDefault() ?? string.Empty,
                        nombre = g.Nombre ?? string.Empty,
                        cantidad_miembros = g.CantidadMiembros,
                        max_cant_miembros = g.MaxCantMiembros,
                        id_localizaciones = g.IdLocalizaciones
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

        [HttpGet("GetGrupoDelLider/{idLider}")]
        public async Task<ActionResult<GrupoDTO>> GetGrupoDelLider(int idLider)
        {
            try
            {
                var grupo = await _context.Grupos
                    .Include(g => g.PerteneceGrupos)
                        .ThenInclude(pg => pg.IdMiembrosNavigation)
                    .Where(g => g.PerteneceGrupos.Any(pg => pg.IdMiembros == idLider && pg.Ocupacion == "Líder"))
                    .Select(g => new GrupoDTO
                    {
                        id_grupo = g.IdGrupos,
                        id_lider = idLider,
                        nombre = g.Nombre ?? string.Empty,
                        cantidad_miembros = g.CantidadMiembros,
                        max_cant_miembros = g.MaxCantMiembros,
                    })
                    .FirstOrDefaultAsync();
                if (grupo == null)
                {
                    return NotFound("El líder no está asignado a ningún grupo");
                }
                return Ok(grupo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el grupo del líder con ID {idLider}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetMiembrosDelGrupo/{id}")]
        public async Task<ActionResult<List<InfoMiembroGrupoDTO>>> GetMiembrosDelGrupo(int id)
        {
            try
            {
                var miembros = await _context.PerteneceGrupos
                    .Where(pg => pg.IdGrupos == id)
                    .Include(pg => pg.IdMiembrosNavigation)
                        .ThenInclude(m => m.Usuario)
                            .ThenInclude(u => u.IdRols)
                    .Select(pg => new InfoMiembroGrupoDTO
                    {
                        id_miembro = pg.IdMiembros,
                        nombre_completo = (pg.IdMiembrosNavigation.Nombre ?? string.Empty) + " " + (pg.IdMiembrosNavigation.Apellido ?? string.Empty),
                        dni = pg.IdMiembrosNavigation.Dni,
                        ocupacion = pg.Ocupacion ?? string.Empty,
                        fecha_desde = pg.FechaDesde,
                        tiene_usuario = pg.IdMiembrosNavigation.Usuario != null,
                        puede_ser_lider = pg.IdMiembrosNavigation.Usuario != null &&
                                          pg.IdMiembrosNavigation.Usuario.IdRols.Any(r => r.Nombre.ToLower() == "lider") &&
                                          !_context.Lideres.Any(l => l.IdMiembros == pg.IdMiembros)
                    })
                    .ToListAsync();
                return Ok(miembros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener los miembros del grupo con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("CambiarOcupacionMiembro/{idMiembro}")]
        public async Task<IActionResult> CambiarOcupacionMiembro(int idMiembro, [FromBody] string request)
        {
            try
            {
                var pertenece = await _context.PerteneceGrupos
                    .FirstOrDefaultAsync(pg => pg.IdMiembros == idMiembro);
                if (pertenece == null)
                {
                    return NotFound($"No se encontró el miembro con ID {idMiembro} en ningún grupo.");
                }
                pertenece.Ocupacion = request;
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("QuitarMiembroDelGrupo/{idGrupo}/{idMiembro}")]
        public async Task<IActionResult> QuitarMiembroDelGrupo(int idGrupo, int idMiembro)
        {
            try
            {
                var pertenece = await _context.PerteneceGrupos
                    .FirstOrDefaultAsync(mg => mg.IdGrupos == idGrupo && mg.IdMiembros == idMiembro);
                if (pertenece == null)
                {
                    return NotFound($"No se encontró el miembro con ID {idMiembro} en ningún grupo.");
                }
                _context.PerteneceGrupos.Remove(pertenece);

                var grupo = await _context.Grupos.FindAsync(idGrupo);
                if (grupo != null && grupo.CantidadMiembros > 0)
                {
                    grupo.CantidadMiembros--;
                }

                await _context.SaveChangesAsync();
                return Ok(new
                {
                    mensaje = "Miembro eliminado del grupo correctamente",
                    nuevaCantidadMiembros = grupo?.CantidadMiembros
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
