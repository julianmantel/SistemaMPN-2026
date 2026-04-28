using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Hubs;
using SistemaMPN.Modules.Autentificacion.Models;
using SistemaMPN.Modules.Autentificacion.Services;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;
using System.Security.Cryptography;
using System.Text;
using static SistemaMPN.Client.Modules.Tesoreria.Pages.DetalleCaja;

namespace SistemaMPN.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(
            DataContext context,
            IAuthService authService,
            ILogger<UsuarioController> logger,
            IHubContext<NotificacionesHub> hubContext)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        // Verificar si un usuario tiene grupo asignado
        [HttpGet("TieneGrupoAsignado/{userName}")]
        [Authorize(Roles = "admin,gestor de miembros,auditor")]
        public async Task<ActionResult<bool>> TieneGrupoAsignado(string userName)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.IdMiembrosNavigation)
                    .FirstOrDefaultAsync(u => u.UserName == userName);

                if (usuario?.IdMiembrosNavigation == null)
                {
                    return Ok(false);
                }

                // Verificar si el miembro es líder de algún grupo
                var esLider = await _context.Lideres
                    .AnyAsync(l => l.IdMiembros == usuario.IdMiembrosNavigation.IdMiembros);

                return Ok(esLider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar si el usuario {userName} tiene grupo asignado");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetInfoMiembros")]
        [Authorize(Roles = "admin,gestor de miembros,auditor")]
        public async Task<ActionResult<List<MiembroDTO>>> GetInfoMiembros()
        {
            try
            {
                var lista = await _context.Miembros
                    .Select(m => new InfoBasicaMiembroDTO { nombre = m.Nombre, apellido = m.Apellido, dni = m.Dni })
                    .ToListAsync();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de miembros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetIdMiembroDelUsuario/{userName}")]
        [Authorize]
        public async Task<ActionResult<int>> GetIdMiembroDelUsuario(string userName)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());

                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                // Verificar que el usuario tiene un IdMiembros válido
                if (usuario.IdMiembros == null || usuario.IdMiembros == 0)
                {
                    return BadRequest("El usuario no tiene un miembro asociado");
                }

                return Ok(usuario.IdMiembros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ID de miembro del usuario {UserName}", userName);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetRoles")]
        [Authorize(Roles = "admin,gestor de miembros,auditor")]
        public async Task<ActionResult<List<RolDTO>>> GetRoles()
        {
            try
            {
                var lista = await _context.Roles.Select(r => new RolDTO { id = r.IdRol, nombre = r.Nombre }).ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetUsuarios")]
        [Authorize(Roles = "admin,gestor de miembros,auditor")]
        public async Task<ActionResult<List<UsuarioDTO>>> GetUsuarios()
        {
            try
            {
                var listUsuarios = await _context.Usuarios
                .Include(u => u.IdRols)
                .Select(u => new UsuarioDTO
                {
                    correo = u.Correo,
                    user_name = u.UserName,
                    roles = u.IdRols.Select(r => (new RolDTO { id = r.IdRol, nombre = r.Nombre })).ToList(),
                    miembro_dni = u.IdMiembrosNavigation.Dni
                })
                .ToListAsync();
                return Ok(listUsuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetSupervisores")]
        [Authorize(Roles = "admin,auditor,tesorero,protesorero")]
        public async Task<ActionResult<List<SupervisorDTO>>> GetSupervisores()
        {
            var supervisores = await _context.Tesoreros
                .Include(t => t.IdMiembrosNavigation)
                .Select(t => new SupervisorDTO
                {
                    Id = t.IdMiembros,
                    Nombre = $"{t.IdMiembrosNavigation.Nombre} {t.IdMiembrosNavigation.Apellido}",
                    Cargo = t.IsPro ? "Protesorero" : "Tesorero"
                })
                .ToListAsync();

            return Ok(supervisores);
        }

        [HttpPost("Modificar")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<ActionResult<string>> ModificarUsuario(ChangeUsuario usuarioDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.IdRols)
                    .Include(u => u.IdMiembrosNavigation)
                    .FirstOrDefaultAsync(u => u.UserName == usuarioDTO.old_user_name);

                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                var usuarioExistente = await _context.Usuarios
                    .AnyAsync(x => x.UserName == usuarioDTO.new_user_name && x.UserName != usuarioDTO.old_user_name);

                var correoExistente = await _context.Usuarios
                    .AnyAsync(x => x.Correo == usuarioDTO.correo && x.UserName != usuarioDTO.old_user_name);

                if (usuarioExistente)
                {
                    return BadRequest("El nombre de usuario ya está en uso");
                }

                if (correoExistente)
                {
                    return BadRequest("El correo ya está en uso");
                }

                if (usuario.IdMiembrosNavigation == null)
                {
                    return BadRequest("El usuario no tiene un miembro asignado");
                }

                var idMiembro = usuario.IdMiembrosNavigation.IdMiembros;

                // VALIDACIÓN: Verificar si está quitando el rol de líder
                var rolesAnteriores = usuario.IdRols.Select(r => r.Nombre).ToList();
                var tieneLiderActualmente = rolesAnteriores.Any(r => r.Equals("lider", StringComparison.OrdinalIgnoreCase));

                var nuevosRolesNombres = new List<string>();
                if (usuarioDTO.roles != null && usuarioDTO.roles.Any())
                {
                    nuevosRolesNombres = usuarioDTO.roles.Select(r => r.nombre).ToList();
                }

                var mantieneLider = nuevosRolesNombres.Any(r => r.Equals("lider", StringComparison.OrdinalIgnoreCase));

                // Si está quitando el rol de líder, verificar si tiene grupo asignado
                if (tieneLiderActualmente && !mantieneLider)
                {
                    var esLiderDeGrupo = await _context.Lideres
                        .AnyAsync(l => l.IdMiembros == idMiembro);

                    if (esLiderDeGrupo)
                    {
                        return BadRequest("No se puede quitar el rol de líder porque este usuario está liderando un grupo. Primero transfiera el liderazgo del grupo.");
                    }
                }

                usuario.UserName = usuarioDTO.new_user_name;
                usuario.Correo = usuarioDTO.correo;

                usuario.IdRols.Clear();

                if (usuarioDTO.roles != null && usuarioDTO.roles.Any())
                {
                    var rolsIds = usuarioDTO.roles.Select(s => s.id).ToList();
                    var nuevosRoles = await _context.Roles
                        .Where(r => rolsIds.Contains(r.IdRol))
                        .ToListAsync();

                    foreach (var rol in nuevosRoles)
                    {
                        usuario.IdRols.Add(rol);
                    }
                }

                // 1. Remover roles que ya no tiene (pasando la lista de nuevos roles para validar)
                foreach (var rolAnterior in rolesAnteriores)
                {
                    if (!nuevosRolesNombres.Contains(rolAnterior, StringComparer.OrdinalIgnoreCase))
                    {
                        await RemoverRolEspecifico(rolAnterior, idMiembro, nuevosRolesNombres);
                    }
                }

                // 2. Agregar o Actualizar roles nuevos
                foreach (var nuevoRol in nuevosRolesNombres)
                {
                    await AsignarRolEspecifico(nuevoRol, idMiembro);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Usuario modificado exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al modificar usuario");
                return StatusCode(500, "Error interno al modificar usuario: " + ex.Message);
            }
        }

        private async Task RemoverRolEspecifico(string nombreRol, int idMiembro, List<string> nuevosRoles)
        {
            switch (nombreRol.ToLower())
            {
                case "consultor":
                    var consultor = await _context.Consultores
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.IdMiembros == idMiembro);
                    if (consultor != null) _context.Consultores.Remove(consultor);
                    break;

                case "gestor de miembros":
                    var gestorMiembro = await _context.GestoresMiembros
                        .AsNoTracking()
                        .FirstOrDefaultAsync(g => g.IdMiembros == idMiembro);
                    if (gestorMiembro != null) _context.GestoresMiembros.Remove(gestorMiembro);
                    break;

                case "lider":
                    // VALIDACIÓN ADICIONAL: No permitir remover líder si tiene grupo
                    var esLiderDeGrupo = await _context.Lideres
                        .AnyAsync(l => l.IdMiembros == idMiembro);

                    if (esLiderDeGrupo)
                    {
                        _logger.LogWarning($"Intento de remover rol de líder del miembro {idMiembro} que tiene grupo asignado");
                        // No removemos el registro de Lideres aquí porque ya se validó antes
                        // Esta validación es una capa adicional de seguridad
                    }
                    break;

                case "tesorero":
                case "protesorero":
                    bool sigueSiendoTesoreria = nuevosRoles.Any(r =>
                        r.Equals("tesorero", StringComparison.OrdinalIgnoreCase) ||
                        r.Equals("protesorero", StringComparison.OrdinalIgnoreCase));

                    if (!sigueSiendoTesoreria)
                    {
                        var tesorero = await _context.Tesoreros
                            .AsNoTracking()
                            .FirstOrDefaultAsync(t => t.IdMiembros == idMiembro);
                        if (tesorero != null)
                        {
                            _context.Tesoreros.Remove(tesorero);
                            _logger.LogInformation($"Registro eliminado de tabla Tesoreros para miembro {idMiembro}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Se conserva registro en tabla Tesoreros para {idMiembro} debido a cambio de rango (Tesorero <-> Protesorero)");
                    }
                    break;

                case "admin":
                    _logger.LogInformation($"Rol admin removido para miembro {idMiembro} (sin tabla específica)");
                    break;

                default:
                    _logger.LogInformation($"Rol {nombreRol} no requiere eliminación de tabla específica");
                    break;
            }
        }

        [HttpDelete("Eliminar/{userName}")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<ActionResult<string>> EliminarUsuario(string userName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.IdRols)
                    .Include(u => u.IdMiembrosNavigation)
                    .FirstOrDefaultAsync(u => u.UserName == userName);

                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                // VALIDACIÓN: Verificar si el usuario tiene un grupo asignado
                if (usuario.IdMiembrosNavigation != null)
                {
                    var esLider = await _context.Lideres
                        .AnyAsync(l => l.IdMiembros == usuario.IdMiembrosNavigation.IdMiembros);

                    if (esLider)
                    {
                        return BadRequest("No se puede eliminar este usuario porque es líder de un grupo. Primero transfiera el liderazgo.");
                    }
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Usuario eliminado exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al eliminar usuario {userName}");
                return StatusCode(500, "Error interno al eliminar usuario: " + ex.Message);
            }
        }

        [HttpPost("Registrar")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<ActionResult<string>> Registrar(UsuarioDTO usuarioDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cuentaExistente = await _context.Usuarios
                    .AnyAsync(x => x.UserName == usuarioDTO.user_name || x.Correo == usuarioDTO.correo);
                if (cuentaExistente)
                {
                    return BadRequest("El nombre y/o correo de usuario ya está en uso");
                }

                Miembro miembro = null;
                if (!string.IsNullOrEmpty(usuarioDTO.miembro_dni))
                {
                    var dni = usuarioDTO.miembro_dni.Split('-')[0].Trim();
                    miembro = await _context.Miembros
                        .FirstOrDefaultAsync(x => x.Dni == dni);
                    if (miembro == null)
                    {
                        return BadRequest("No se encontró el miembro especificado");
                    }
                }
                else
                {
                    return BadRequest("Debe seleccionar un miembro para el usuario");
                }

                // Crear nuevo usuario
                var usuario = new Usuario();
                _authService.CreatePasswordHash(usuarioDTO.password, out byte[] passwordHash, out byte[] passwordSalt);
                usuario.UserName = usuarioDTO.user_name;
                usuario.Correo = usuarioDTO.correo;
                usuario.PasswordHash = passwordHash;
                usuario.PasswordSalt = passwordSalt;
                usuario.IdMiembrosNavigation = miembro;

                // Asignar roles y crear registros en tablas específicas
                if (usuarioDTO.roles != null && usuarioDTO.roles.Any())
                {
                    var rolsIds = usuarioDTO.roles.Select(s => s.id).ToList();
                    var roles = await _context.Roles
                        .Where(r => rolsIds.Contains(r.IdRol))
                        .ToListAsync();

                    foreach (var rol in roles)
                    {
                        rol.IdUsuarios.Add(usuario);

                        await AsignarRolEspecifico(rol.Nombre, miembro.IdMiembros);
                    }
                }
                else
                {
                    return BadRequest("Debe asignar al menos un rol al usuario");
                }

                // Guardar cambios
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Usuario registrado exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, "Error interno al registrar usuario: " + ex.Message);
            }
        }

        private async Task AsignarRolEspecifico(string nombreRol, int idMiembro)
        {
            switch (nombreRol.ToLower())
            {
                case "consultor":
                    var consultorExistente = await _context.Consultores
                        .AnyAsync(c => c.IdMiembros == idMiembro);
                    if (!consultorExistente)
                    {
                        _context.Consultores.Add(new Consultor { IdMiembros = idMiembro });
                    }
                    break;

                case "gestor de miembros":
                    var gestorExistente = await _context.GestoresMiembros
                        .AnyAsync(g => g.IdMiembros == idMiembro);
                    if (!gestorExistente)
                    {
                        _context.GestoresMiembros.Add(new GestorMiembro { IdMiembros = idMiembro });
                    }
                    break;

                case "tesorero":
                case "protesorero":
                    bool isPro = nombreRol.ToLower() == "protesorero";

                    // Buscar primero en el contexto local (entidades ya tracked)
                    var tesorero = _context.Tesoreros.Local
                        .FirstOrDefault(t => t.IdMiembros == idMiembro);

                    // Si no está en el contexto local, buscar en BD
                    if (tesorero == null)
                    {
                        tesorero = await _context.Tesoreros
                            .FirstOrDefaultAsync(t => t.IdMiembros == idMiembro);
                    }

                    if (tesorero != null)
                    {
                        // Ya existe, solo actualizar
                        tesorero.IsPro = isPro;
                    }
                    else
                    {
                        // No existe, crear nuevo
                        _context.Tesoreros.Add(new Tesorero
                        {
                            IdMiembros = idMiembro,
                            IsPro = isPro
                        });
                    }
                    break;

                default:
                    _logger.LogInformation($"Rol {nombreRol} no requiere tabla específica");
                    break;
            }
        }
    }
}