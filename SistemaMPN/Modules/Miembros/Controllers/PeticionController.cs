using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Controllers;
using SistemaMPN.Data;
using SistemaMPN.Hubs;
using SistemaMPN.Modules.Autentificacion.Services;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Miembros.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PeticionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<UsuarioController> _logger;
        private readonly INotificacionService _notificacionService;
        public PeticionController(DataContext context, ILogger<UsuarioController> logger, INotificacionService notificacionService)
        {
            _context = context;
            _logger = logger;
            _notificacionService = notificacionService;
        }

        [HttpGet("GetPeticiones")]
        [Authorize(Roles = "admin,gestor de miembros,consultor,auditor")]
        public async Task<ActionResult<List<PeticionDTO>>> GetPeticiones()
        {
            int? usuarioId = _context.Usuarios
                                         .Where(u => u.UserName.Equals(User.FindFirst(ClaimTypes.Name)!.Value))
                                         .Select(u => u.IdMiembros).First();

            if (usuarioId == null) return StatusCode(404, "Usuario no encontrado.");

            // Las peticiones de tipo "Reunion" solo son visibles para usuarios con rol "consultor"
            bool tieneRolConsultor = User.IsInRole("consultor");

            var queryPeticiones = _context.Peticiones.AsQueryable();

            // Si NO tiene el rol consultor, excluir peticiones de tipo "Reunion"
            if (!tieneRolConsultor)
            {
                queryPeticiones = queryPeticiones.Where(p => p.Tipo != "Reunion");
            } else
            {
                queryPeticiones = queryPeticiones.Where(p => p.Tipo == "Reunion");
            }

            var peticiones = await queryPeticiones
                    .OrderByDescending(p => p.FechaSolicitud)
                    .Select(p => new PeticionDTO
                    {
                        IdPeticiones = p.IdPeticiones,
                        FechaSolicitud = p.FechaSolicitud,
                        FechaRespuesta = p.FechaRespuesta,
                        Estado = p.Estado,
                        Mensaje = p.Mensaje,
                        Tipo = p.Tipo,
                        IdUsuario = p.IdUsuario,
                        IdLider = p.IdLider
                    })
                    .ToListAsync();
            return Ok(peticiones);
        }

        [HttpPost("RegistrarPeticionCambio")]
        [Authorize(Roles = "admin,gestor de miembros,lider")]
        public async Task<ActionResult<string>> RegistrarPeticionCambio(PeticionCambioDTO peticionCambioDTO)
        {
            try
            {
                var existePeticion = await _context.PeticionesCambio
                .AnyAsync(x => (x.IdMiembros == peticionCambioDTO.IdMiembros && x.IdGrupos == peticionCambioDTO.IdGrupos) && ((Peticion)x.IdPeticionesNavigation).Estado == "Pendiente");
                if (existePeticion)
                {
                    return BadRequest("La peticion ya está hecha");
                }

                var peticion = new Peticion
                {
                    FechaSolicitud = peticionCambioDTO.Peticion.FechaSolicitud,
                    IdLider = peticionCambioDTO.Peticion.IdLider,
                    Mensaje = peticionCambioDTO.Peticion.Mensaje,
                    Estado = "Pendiente",
                    Tipo = peticionCambioDTO.Peticion.Tipo
                };

                var peticionCambio = new PeticionCambio();
                peticionCambio.IdMiembros = peticionCambioDTO.IdMiembros;
                peticionCambio.IdGrupos = peticionCambioDTO.IdGrupos;
                _context.Add(peticionCambio);

                peticion.PeticionCambio = peticionCambio;
                _context.Add(peticion);

                await _context.SaveChangesAsync();
                await _notificacionService.EnviarNotificacionPeticion(peticionCambioDTO.Peticion);

                return Ok("Petición de cambio registrada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la petición de cambio.");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPost("RegistrarPeticionActualizacion")]
        [Authorize(Roles = "admin,gestor de miembros,lider")]
        public async Task<ActionResult<string>> RegistrarPeticionActualizacion(PeticionActualizacionDTO peticionActualizacionDTO)
        {
            try
            {
                var existePeticion = await _context.PeticionesActualizacion
                .AnyAsync(x => x.IdMiembros == peticionActualizacionDTO.IdMiembros && (x.IdPeticionesNavigation as Peticion).Estado == "Pendiente");

                if (existePeticion)
                {
                    return BadRequest("La peticion ya está hecha");
                }

                var peticion = new Peticion
                {
                    FechaSolicitud = peticionActualizacionDTO.Peticion.FechaSolicitud,
                    IdLider = peticionActualizacionDTO.Peticion.IdLider,
                    Mensaje = peticionActualizacionDTO.Peticion.Mensaje,
                    Estado = "Pendiente",
                    Tipo = peticionActualizacionDTO.Peticion.Tipo
                };

                var peticionActualizacion = new PeticionActualizacion();
                peticionActualizacion.IdMiembros = peticionActualizacionDTO.IdMiembros;
                _context.Add(peticionActualizacion);

                peticion.PeticionActualizacion = peticionActualizacion;

                _context.Add(peticion);
                await _context.SaveChangesAsync();
                await _notificacionService.EnviarNotificacionPeticion(peticionActualizacionDTO.Peticion);

                return Ok("Petición de actualización registrada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la petición de actualización.");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPost("RegistrarPeticionAgregarMiembro")]
        [Authorize(Roles = "admin,gestor de miembros,lider")]
        public async Task<ActionResult<string>> RegistrarPeticionAgregarMiembro(PeticionAgregarDTO peticionAgregarDTO)
        {
            try
            {
                var nroPeticiones = _context.Peticiones.Where(x => x.IdLider == peticionAgregarDTO.Peticion.IdLider && x.Estado == "Pendiente" && x.Tipo == "Agregar").Count();
                if(nroPeticiones >= 5)
                {
                    return BadRequest("Espera hasta que un gestor de miembros responda las peticiones pendientes");
                }
                
                var peticion = new Peticion
                {   
                    FechaSolicitud = peticionAgregarDTO.Peticion.FechaSolicitud,
                    IdLider = peticionAgregarDTO.Peticion.IdLider,
                    Mensaje = peticionAgregarDTO.Peticion.Mensaje,
                    Estado = "Pendiente",
                    Tipo = peticionAgregarDTO.Peticion.Tipo
                };

                var peticionAgregar = new PeticionAgregar
                {
                    Dni = peticionAgregarDTO.Dni,
                    Nombre = peticionAgregarDTO.Nombre,
                    Apellido = peticionAgregarDTO.Apellido,
                    FechaNacimiento = peticionAgregarDTO.FechaNacimiento,
                    Nacionalidad = peticionAgregarDTO.Nacionalidad,
                    LugarNacimiento = peticionAgregarDTO.LugarNacimiento,
                    Telefono = peticionAgregarDTO.Telefono,
                    Sexo = peticionAgregarDTO.Sexo
                };

                peticion.PeticionAgregar = peticionAgregar;
                _context.Add(peticion);
                await _context.SaveChangesAsync();
                await _notificacionService.EnviarNotificacionPeticion(peticionAgregarDTO.Peticion);

                return Ok("Petición para agregar miembro registrada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la petición para agregar miembro.");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPost("RegistrarPeticionReunion")]
        [Authorize(Roles = "admin,gestor de miembros,lider")]
        public async Task<IActionResult> RegistrarPeticionReunion(PeticionReunionDTO peticionReunionDTO)
        {
            try
            {
                var peticion = new Peticion
                {
                    FechaSolicitud = peticionReunionDTO.Peticion.FechaSolicitud,
                    IdLider = peticionReunionDTO.Peticion.IdLider,
                    Mensaje = peticionReunionDTO.Peticion.Mensaje,
                    Estado = "Pendiente",
                    Tipo = peticionReunionDTO.Peticion.Tipo
                };

                var peticionReunion = new PeticionReunion
                {
                    Motivo = peticionReunionDTO.Motivo,
                    FechaPreferida = peticionReunionDTO.FechaPreferida,
                    Correo = peticionReunionDTO.Correo,
                    IdMiembros = peticionReunionDTO.IdMiembros
                };

                _context.Add(peticionReunion);

                peticion.PeticionReunion = peticionReunion;
                _context.Add(peticion);

                await _notificacionService.EnviarNotificacionPeticion(peticionReunionDTO.Peticion);

                return Ok(new { mensaje = "Petición de reunión registrada exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la petición de reunión.");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpGet("GetPeticionDetalle/{idPeticion}")]
        [Authorize(Roles = "admin,gestor de miembros,consultor,auditor")]
        public async Task<IActionResult> GetPeticionDetalle(int idPeticion)
        {
            var peticion = await _context.Peticiones
                .Include(p => p.IdLiderNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .FirstOrDefaultAsync(p => p.IdPeticiones == idPeticion);

            if (peticion == null)
                return NotFound();

            switch (peticion.Tipo)
            {
                case "Reunion":
                    var reunion = await _context.PeticionesReunion
                        .Include(r => r.IdMiembrosNavigation)
                        .FirstOrDefaultAsync(r => r.IdPeticiones == idPeticion);
                    if (reunion != null)
                    {
                        return Ok(new PeticionReunionDetalleDTO
                        {
                            IdPeticiones = peticion.IdPeticiones,
                            Tipo = peticion.Tipo ?? string.Empty,
                            FechaSolicitud = peticion.FechaSolicitud,
                            FechaRespuesta = peticion.FechaRespuesta,
                            Estado = peticion.Estado,
                            Mensaje = peticion.Mensaje,
                            NombreSolicitante = peticion.IdLiderNavigation != null
                                ? $"{peticion.IdLiderNavigation.Nombre} {peticion.IdLiderNavigation.Apellido}"
                                : $"Líder #{peticion.IdLider}",
                            NombreQuienResponde = peticion.IdUsuarioNavigation?.UserName,
                            Motivo = reunion.Motivo,
                            FechaPreferida = reunion.FechaPreferida,
                            Correo = reunion.Correo,
                            IdMiembro = reunion.IdMiembros,
                            NombreMiembro = reunion.IdMiembrosNavigation != null
                                ? $"{reunion.IdMiembrosNavigation.Nombre} {reunion.IdMiembrosNavigation.Apellido}"
                                : null
                        });
                    }
                    break;

                case "Cambio":
                    var cambio = await _context.PeticionesCambio
                        .Include(c => c.IdMiembrosNavigation)
                        .Include(c => c.IdGruposNavigation)
                        .FirstOrDefaultAsync(c => c.IdPeticiones == idPeticion);
                    if (cambio != null)
                    {
                        return Ok(new PeticionCambioDetalleDTO
                        {
                            IdPeticiones = peticion.IdPeticiones,
                            Tipo = peticion.Tipo ?? string.Empty,
                            FechaSolicitud = peticion.FechaSolicitud,
                            FechaRespuesta = peticion.FechaRespuesta,
                            Estado = peticion.Estado,
                            Mensaje = peticion.Mensaje,
                            NombreSolicitante = peticion.IdLiderNavigation != null
                                ? $"{peticion.IdLiderNavigation.Nombre} {peticion.IdLiderNavigation.Apellido}"
                                : $"Líder #{peticion.IdLider}",
                            NombreQuienResponde = peticion.IdUsuarioNavigation?.UserName,
                            IdMiembro = cambio.IdMiembros,
                            NombreMiembro = cambio.IdMiembrosNavigation != null
                                ? $"{cambio.IdMiembrosNavigation.Nombre} {cambio.IdMiembrosNavigation.Apellido}"
                                : null,
                            IdGrupo = cambio.IdGrupos,
                            NombreGrupo = cambio.IdGruposNavigation?.Nombre
                        });
                    }
                    break;

                case "Actualizacion":
                    var actualizacion = await _context.PeticionesActualizacion
                        .Include(a => a.IdMiembrosNavigation)
                        .FirstOrDefaultAsync(a => a.IdPeticiones == idPeticion);
                    if (actualizacion != null)
                    {
                        return Ok(new PeticionActualizacionDetalleDTO
                        {
                            IdPeticiones = peticion.IdPeticiones,
                            Tipo = peticion.Tipo ?? string.Empty,
                            FechaSolicitud = peticion.FechaSolicitud,
                            FechaRespuesta = peticion.FechaRespuesta,
                            Estado = peticion.Estado,
                            Mensaje = peticion.Mensaje,
                            NombreSolicitante = peticion.IdLiderNavigation != null
                                ? $"{peticion.IdLiderNavigation.Nombre} {peticion.IdLiderNavigation.Apellido}"
                                : $"Líder #{peticion.IdLider}",
                            NombreQuienResponde = peticion.IdUsuarioNavigation?.UserName,
                            IdMiembro = actualizacion.IdMiembros,
                            NombreMiembro = actualizacion.IdMiembrosNavigation != null
                                ? $"{actualizacion.IdMiembrosNavigation.Nombre} {actualizacion.IdMiembrosNavigation.Apellido}"
                                : null
                        });
                    }
                    break;

                case "Agregar":
                    var agregar = await _context.PeticionesAgregar
                        .FirstOrDefaultAsync(a => a.IdPeticiones == idPeticion);
                    if (agregar != null)
                    {
                        return Ok(new PeticionAgregarDetalleDTO
                        {
                            IdPeticiones = peticion.IdPeticiones,
                            Tipo = peticion.Tipo ?? string.Empty,
                            FechaSolicitud = peticion.FechaSolicitud,
                            FechaRespuesta = peticion.FechaRespuesta,
                            Estado = peticion.Estado,
                            Mensaje = peticion.Mensaje,
                            NombreSolicitante = peticion.IdLiderNavigation != null
                                ? $"{peticion.IdLiderNavigation.Nombre} {peticion.IdLiderNavigation.Apellido}"
                                : $"Líder #{peticion.IdLider}",
                            NombreQuienResponde = peticion.IdUsuarioNavigation?.UserName,
                            Nombre = agregar.Nombre,
                            Apellido = agregar.Apellido,
                            Dni = agregar.Dni,
                            Nacionalidad = agregar.Nacionalidad,
                            FechaNacimiento = agregar.FechaNacimiento,
                            LugarNacimiento = agregar.LugarNacimiento,
                            Telefono = agregar.Telefono,
                            Sexo = agregar.Sexo
                        });
                    }
                    break;
            }

            return NotFound();
        }

        [HttpPatch("CambiarEstado/{idPeticion}/{nuevoEstado}")]
        [Authorize(Roles = "admin,gestor de miembros,consultor")]
        public async Task<IActionResult> Aceptar(int idPeticion, string nuevoEstado)
        {

            int? usuarioId = _context.Usuarios
                                         .Where(u => u.UserName.Equals(User.FindFirst(ClaimTypes.Name)!.Value))
                                         .Select(u => u.IdUsuarios).First();

            nuevoEstado = nuevoEstado.Trim();

            if (!nuevoEstado.Equals("Aceptada") && !nuevoEstado.Equals("Rechazada"))
            {
                return BadRequest("Estado inválido.");
            }

            try
            {
                var peticion = await _context.Peticiones.FindAsync(idPeticion);
                if (peticion == null)
                {
                    return NotFound();
                }

                if (!peticion.Estado.Equals("Pendiente"))
                {
                    return BadRequest("La petición ya ha sido respondida.");
                }

                peticion.Estado = nuevoEstado;
                peticion.FechaRespuesta = DateTime.UtcNow;
                peticion.IdUsuario = usuarioId;

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aceptar la petición.");
                return StatusCode(500, "Error interno del servidor.");
            }
        }
    }
}
