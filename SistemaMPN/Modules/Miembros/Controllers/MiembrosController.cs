using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaMPN.Controllers;
using SistemaMPN.Data;
using SistemaMPN.Modules.Miembros.Services;
using SistemaMPN.Modules.Archivos.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;
using System.Text.Json;

namespace SistemaMPN.Modules.Miembros.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
public class MiembrosController : ControllerBase
    {
        private readonly IMiembroService _miembroService;
        private readonly DataContext _context;
        private readonly ILogger<MiembrosController> _logger;
        private readonly IMegaStorageService _megaStorageService;
        public MiembrosController(IMiembroService miembroService, DataContext context, ILogger<MiembrosController> logger, IMegaStorageService megaStorageService)
        {
            _miembroService = miembroService;
            _context = context;
            _logger = logger;
            _megaStorageService = megaStorageService;
        }

        [HttpPost("RegistrarMiembro")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<IActionResult> Registrar(
                    [FromForm] string miembroJson,
                    [FromForm] IFormFile? foto)
        {
            try
            {
                var miembroDTO = JsonSerializer.Deserialize<MiembroDTO>(miembroJson);

                if (miembroDTO == null)
                    return BadRequest("Datos inválidos");

                var miembro = new Miembro();
                miembro = _miembroService.CargarDatosBasicosMiembro(miembro, miembroDTO.info_basica_miembro);

                var dniExistente = await _context.Miembros.AnyAsync(m => m.Dni == miembro.Dni);
                if (dniExistente)
                    return BadRequest("El dni coincide con el de otro miembro");

                if (_miembroService.TieneInformacionPersonal(miembroDTO.info_personal))
                {
                    var datosPersonales = _miembroService.CargarDatosPersonales(miembroDTO.info_personal);

                    foreach (var hijo in miembroDTO.info_personal.hijos)
                    {
                        var hijoExistente = await _context.Hijos
                            .FirstOrDefaultAsync(h => h.Nombre == hijo.nombre && h.Apellido == hijo.apellido);

                        Hijo nuevoHijo;

                        if (hijoExistente != null)
                        {
                            nuevoHijo = hijoExistente; // ya existe, no lo insertamos
                        }
                        else
                        {
                            nuevoHijo = new Hijo
                            {
                                Nombre = hijo.nombre,
                                Apellido = hijo.apellido
                            };
                            _context.Hijos.Add(nuevoHijo); // insertamos uno nuevo
                        }

                        datosPersonales.IdHijos.Add(nuevoHijo);
                    }

                    _context.Add(datosPersonales);
                    miembro.IdDatosPersonalesNavigation = datosPersonales;
                }

                //trayectoria
                if (_miembroService.TieneInformacionAcademica(miembroDTO.info_academica) || _miembroService.TieneInformacionLaboral(miembroDTO.info_laboral))
                {
                    var trayectoria = new Trayectoria();
                    trayectoria = _miembroService.CargarTrayectoria(miembroDTO.info_academica, miembroDTO.info_laboral);
                    _context.Add(trayectoria);
                    miembro.IdTrayectoriaNavigation = trayectoria;
                }

                // Informacion de eclesiastica
                if (_miembroService.TieneInformacionEclesiastica(miembroDTO.info_eclesiastica))
                {
                    var infoEclesiastica = _miembroService.CargarInfoEclesiastica(miembroDTO.info_eclesiastica);

                    // Cargar Bautismo
                    var bautismo = _miembroService.CargarBautismo(miembroDTO.info_eclesiastica);
                    _context.Add(bautismo);
                    infoEclesiastica.IdBautismoNavigation = bautismo;
                    _context.Add(infoEclesiastica);
                    await _context.SaveChangesAsync();

                    // Inicializamos lista si es null
                    infoEclesiastica.SeminariosCursados ??= new List<SeminariosCursado>();

                    if (miembroDTO.info_eclesiastica.seminarios != null && miembroDTO.info_eclesiastica.seminarios.Any())
                    {
                        foreach (var seminarioDTO in miembroDTO.info_eclesiastica.seminarios)
                        {
                            var seminarioExistente = await _context.Seminarios
                                .FirstOrDefaultAsync(s => s.Nombre.ToLower() == seminarioDTO.nombre.ToLower());

                            if (seminarioExistente == null)
                            {
                                seminarioExistente = new Seminario
                                {
                                    Nombre = seminarioDTO.nombre,
                                    Activo = true,
                                    AnioComienzo = seminarioDTO.anio_comienzo
                                };
                                _context.Add(seminarioExistente);
                                await _context.SaveChangesAsync();
                            }

                            // Crear la relación SeminariosCursado
                            var seminarioCursado = new SeminariosCursado
                            {
                                IdSeminario = seminarioExistente.IdSeminario,
                                IdInformacionEclesiastica = infoEclesiastica.IdInformacionEclesiastica,
                                Estado = seminarioDTO.estado ?? "Cursado",
                                AnioCursado = seminarioDTO.anio_cursado ?? DateTime.Now.Year,
                                IdSeminariosNavigation = seminarioExistente,
                                IdInformacionEclesiasticaNavigation = infoEclesiastica
                            };

                            _context.Add(seminarioCursado);
                        }
                    }

                    miembro.IdInformacionEclesiasticaNavigation = infoEclesiastica;
                }

                // Informacion de salud
                if (_miembroService.TieneInformacionSalud(miembroDTO.info_salud))
                {
                    var infoSalud = new InformacionSalud();
                    infoSalud = _miembroService.CargarInfoSalud(miembroDTO.info_salud);
                    foreach (var alergia in miembroDTO.info_salud.alergias)
                    {
                        var alergiaExiste = await _context.Alergias
                            .FirstOrDefaultAsync(a => a.Nombre == alergia);

                        Alergia nuevaAlergia;

                        if (alergiaExiste != null)
                        {
                            nuevaAlergia = alergiaExiste;
                        }
                        else
                        {
                            nuevaAlergia = new Alergia { Nombre = alergia };
                            _context.Alergias.Add(nuevaAlergia);
                        }
                        infoSalud.IdAlergia.Add(nuevaAlergia);
                    }
                    foreach (var medicamento in miembroDTO.info_salud.medicamentos)
                    {
                        var medicamentoExiste = await _context.Medicamentos
                            .FirstOrDefaultAsync(m => m.Nombre == medicamento);

                        Medicamento nuevoMedicamento;

                        if (medicamentoExiste != null)
                        {
                            nuevoMedicamento = medicamentoExiste;
                        }
                        else
                        {
                            nuevoMedicamento = new Medicamento { Nombre = medicamento };
                            _context.Medicamentos.Add(nuevoMedicamento);
                        }
                        infoSalud.IdMedicamentos.Add(nuevoMedicamento);
                    }
                    foreach (var condicionMedica in miembroDTO.info_salud.condiciones_medicas)
                    {
                        var alergiaExiste = await _context.CondicionesMedicas
                            .FirstOrDefaultAsync(c => c.Condicion == condicionMedica);

                        CondicionMedica nuevaCondicion;

                        if (alergiaExiste != null)
                        {
                            nuevaCondicion = alergiaExiste;
                        }
                        else
                        {
                            nuevaCondicion = new CondicionMedica { Condicion = condicionMedica };
                            _context.CondicionesMedicas.Add(nuevaCondicion);
                        }
                        infoSalud.IdCondicionMedicas.Add(nuevaCondicion);
                    }

                    _context.Add(infoSalud);
                    await _context.SaveChangesAsync();

                    foreach (var telefonoEmergencia in miembroDTO.info_salud.telefonosEmergencias)
                    {
                        var telefonoExistente = await _context.TelefonosEmergencias
                                .FirstOrDefaultAsync(t =>
                                    t.Telefono == telefonoEmergencia.nro_telefono &&
                                    t.Propietario == telefonoEmergencia.propietario_telefono &&
                                    t.IdInformacionSalud == infoSalud.IdInformacionSalud);


                        TelefonoEmergencia nuevoTelefonoEmergencia;

                        if (telefonoExistente != null)
                        {
                            nuevoTelefonoEmergencia = telefonoExistente;
                        }
                        else
                        {
                            nuevoTelefonoEmergencia = new TelefonoEmergencia
                            {
                                Telefono = telefonoEmergencia.nro_telefono,
                                Propietario = telefonoEmergencia.propietario_telefono,
                                IdInformacionSalud = infoSalud.IdInformacionSalud,
                                IdInformacionSaludNavigation = infoSalud
                            };


                            _context.TelefonosEmergencias.Add(nuevoTelefonoEmergencia);
                        }
                    }

                    await _context.SaveChangesAsync();

                    miembro.IdInformacionSaludNavigation = infoSalud;

                }

                // Direccion
                if (_miembroService.TieneDireccion(miembroDTO.direccion))
                {
                    var nuevaDireccion = new Direccion();

                    var existeDireccion = await _context.Direcciones
                    .FirstOrDefaultAsync(d =>
                        d.Altura == miembroDTO.direccion.altura &&
                        d.Barrio == miembroDTO.direccion.barrio &&
                        d.Calle == miembroDTO.direccion.calle);
                    if (existeDireccion != null)
                    {
                        nuevaDireccion = existeDireccion;
                    }
                    else
                    {
                        nuevaDireccion = new Direccion
                        {
                            Altura = miembroDTO.direccion.altura,
                            Barrio = miembroDTO.direccion.barrio,
                            Calle = miembroDTO.direccion.calle
                        };
                        _context.Direcciones.Add(nuevaDireccion);
                    }
                    miembro.IdDireccionNavigation = nuevaDireccion;
                }

                _context.Add(miembro);
                await _context.SaveChangesAsync();

                if (foto != null && foto.Length > 0)
                {
                    await SubirFotoPerfil(miembro.IdMiembros, foto);
                }

                return Ok(new { message = "Miembro registrado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar miembro");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/foto")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<IActionResult> SubirFotoPerfil(int id, IFormFile foto)
        {
            try
            {
                if (foto == null || foto.Length == 0)
                {
                    return BadRequest(new { message = "No se proporciono ningún archivo"});
                }

                var extension = Path.GetExtension(foto.FileName).Trim('.').ToLower();
                if (!new[] {"jpg", "jpeg", "png"}.Contains(extension))
                {
                    return BadRequest(new { message = "Formato de imagen no soportado. Formatos admitidos: jpg, jpeg y png"});
                }

                if (foto.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "El archivo es demasiado grande. Tamaño máximo: 10MB" });
                }

                using var stream = foto.OpenReadStream();
                var nombreArchivo = await _megaStorageService.SubirFotoPerfilAsync(id, stream, extension);

                return Ok(new
                {
                    message = "Foto de perfil actualizada exitosamente",
                    nombreArchivo = nombreArchivo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al subir la foto de perfil",
                    error = ex.Message
                });
            }
        }

        [HttpGet("GetInfoMinimaMiembro")]
        [Authorize(Roles = "admin,gestor de miembros,auditor,consultor")]
        public async Task<ActionResult<List<InfoMinimaMiembroDTO>>> GetInfoMinimaMiembro()
        {
            try
            {
                var lista = await _context.Miembros
                    .Select(m => new InfoMinimaMiembroDTO
                    {
                        id = m.IdMiembros,
                        nombre_completo = $"{m.Nombre} {m.Apellido}",
                        dni = m.Dni,
                        telefono = m.Telefono,
                        telefono_fijo = m.TelefonoFijo,
                        fecha_creacion = m.FechaCreacion
                    })
                    .ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información mínima de miembros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetInfoMinimaMiembroPorLider/{liderId}")]
        [Authorize(Roles = "lider, admin, gestor de miembros")]
        public async Task<ActionResult<List<InfoMinimaMiembroDTO>>> GetInfoMinimaMiembroPorLider(int liderId)
        {
            try
            {
                // Verificar que el líder existe y es efectivamente un líder
                var liderExiste = await _context.Lideres.AnyAsync(l => l.IdMiembros == liderId);
                if (!liderExiste)
                {
                    return NotFound($"No se encontró el líder con ID {liderId}");
                }

                // Obtener los grupos donde está el líder
                var gruposDelLider = await _context.PerteneceGrupos
                    .Where(pg => pg.IdMiembros == liderId)
                    .Select(pg => pg.IdGrupos)
                    .ToListAsync();

                if (!gruposDelLider.Any())
                {
                    return Ok(new List<InfoMinimaMiembroDTO>()); // El líder no está en ningún grupo
                }

                // Obtener todos los miembros de esos grupos, excluyendo al líder mismo
                var lista = await _context.Miembros
                    .Where(m => m.PerteneceGrupos.Any(pg => gruposDelLider.Contains(pg.IdGrupos)))
                    .Select(m => new InfoMinimaMiembroDTO
                    {
                        id = m.IdMiembros,
                        nombre_completo = $"{m.Nombre} {m.Apellido}",
                        dni = m.Dni,
                        telefono = m.Telefono,
                        telefono_fijo = m.TelefonoFijo,
                        fecha_creacion = m.FechaCreacion
                    })
                    .ToListAsync();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información mínima de miembros por líder con ID {LiderId}", liderId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetInfoMiembro/{id}")]
        [Authorize(Roles = "admin,gestor de miembros,auditor,lider")]
        public async Task<ActionResult<MiembroDTO>> GetInfoMiembro(int id)
        {
            try
            {
                var miembroExiste = await _context.Miembros.AnyAsync(m => m.IdMiembros == id);

                if (!miembroExiste)
                {
                    return NotFound($"No se encontró el miembro con ID {id}");
                }

                var miembro = await _context.Miembros
                    .Where(m => m.IdMiembros == id)
                    .Select(m => new MiembroDTO
                    {
                        id = m.IdMiembros,
                        fecha_creacion = m.FechaCreacion,
                        info_basica_miembro = new InfoBasicaMiembroDTO
                        {
                            dni = m.Dni,
                            nombre = m.Nombre,
                            apellido = m.Apellido,
                            nacionalidad = m.Nacionalidad,
                            lugarNacimiento = m.LugarNacimiento,
                            fecha_nacimiento = m.FechaNacimiento.ToDateTime(new TimeOnly(0, 0)),
                            telefono = m.Telefono,
                            telefono_fijo = m.TelefonoFijo,
                            sexo = m.Sexo
                        },
                        info_academica = new InformacionAcademicaDTO
                        {
                            id = m.IdTrayectoria,
                            carrera = m.IdTrayectoriaNavigation.Carrera ?? string.Empty,
                            estudios_primario = m.IdTrayectoriaNavigation.EstudiosPrimario ?? string.Empty,
                            estudios_secundario = m.IdTrayectoriaNavigation.EstudiosSecundario ?? string.Empty,
                            estudios_terciario = m.IdTrayectoriaNavigation.EstudiosTerciario ?? string.Empty,
                            estudios_universitario = m.IdTrayectoriaNavigation.EstudiosUniversitario ?? string.Empty,
                            cursos_realizados = m.IdTrayectoriaNavigation.CursosRealizados ?? string.Empty,
                            tiene_informacion_academica = m.IdTrayectoriaNavigation != null
                        },
                        info_salud = new InformacionSaludDTO
                        {
                            id = m.IdInformacionSalud,
                            grupo_sanguineo = m.IdInformacionSaludNavigation.GrupoSanguineo ?? string.Empty,
                            observaciones = m.IdInformacionSaludNavigation.Observaciones ?? string.Empty,
                            alergias = m.IdInformacionSaludNavigation.IdAlergia.Select(a => a.Nombre).ToList() ?? new List<string>(),
                            condiciones_medicas = m.IdInformacionSaludNavigation.IdCondicionMedicas.Select(c => c.Condicion).ToList() ?? new List<string>(),
                            medicamentos = m.IdInformacionSaludNavigation.IdMedicamentos.Select(med => med.Nombre).ToList() ?? new List<string>(),
                            telefonosEmergencias = m.IdInformacionSaludNavigation.TelefonosEmergencia
                                .Select(t => new TelefonoEmergenciaDTO
                                {
                                    nro_telefono = t.Telefono,
                                    propietario_telefono = t.Propietario
                                }).ToList() ?? new List<TelefonoEmergenciaDTO>(),
                            tiene_informacion_salud = m.IdInformacionSaludNavigation != null
                        },
                        info_laboral = new InformacionLaboralDTO
                        {
                            id = m.IdTrayectoria,
                            rubro = m.IdTrayectoriaNavigation.Rubro ?? string.Empty,
                            situacion_laboral = m.IdTrayectoriaNavigation.SituacionLaboral ?? string.Empty,
                            tiene_informacion_laboral = m.IdTrayectoriaNavigation != null
                        },
                        info_eclesiastica = new InformacionEclesiasticaDTO
                        {
                            id = m.IdInformacionEclesiastica,
                            convocante = m.IdInformacionEclesiasticaNavigation.Convocante ?? string.Empty,
                            fecha_desde_la_que_asiste = m.IdInformacionEclesiasticaNavigation.FechaAsiste,
                            realizo_bautismo = m.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Realizo,
                            fecha_de_bautismo = m.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Fecha,
                            pastor = m.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Pastor ?? string.Empty,
                            lugar_bautismo = m.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Lugar ?? string.Empty,
                            seminarios = m.IdInformacionEclesiasticaNavigation.SeminariosCursados
                                .Select(s => new SeminarioDTO
                                {
                                    id = s.IdSeminario,
                                    nombre = s.IdSeminariosNavigation.Nombre ?? string.Empty,
                                    anio_comienzo = s.IdSeminariosNavigation.AnioComienzo,
                                    activo = s.IdSeminariosNavigation.Activo ?? false,
                                    anio_cursado = s.AnioCursado,
                                    estado = s.Estado ?? string.Empty
                                }).ToList() ?? new List<SeminarioDTO>(),
                            tiene_informacion_eclesiastica = m.IdInformacionEclesiasticaNavigation != null
                        },
                        info_personal = new InformacionPersonalDTO
                        {
                            id = m.IdDatosPersonales,
                            estado_civil = m.IdDatosPersonalesNavigation.EstadoCivil ?? string.Empty,
                            hijos = m.IdDatosPersonalesNavigation.IdHijos
                                .Select(h => new HijoDTO
                                {
                                    nombre = h.Nombre ?? string.Empty,
                                    apellido = h.Apellido ?? string.Empty
                                }).ToList() ?? new List<HijoDTO>(),
                            pareja = m.IdDatosPersonalesNavigation.Pareja ?? string.Empty,
                            telefono_alternativo = m.IdDatosPersonalesNavigation.TelefonoAlternativo ?? string.Empty,
                            tiene_informacion_personal = m.IdDatosPersonalesNavigation != null
                        },
                        direccion = new DireccionDTO
                        {
                            id = m.IdDireccion,
                            altura = m.IdDireccionNavigation.Altura,
                            barrio = m.IdDireccionNavigation.Barrio ?? string.Empty,
                            calle = m.IdDireccionNavigation.Calle ?? string.Empty,
                            tiene_direccion = m.IdDireccionNavigation != null
                        }
                    })
                    .FirstOrDefaultAsync();                    

                return Ok(miembro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información del miembro con ID {MiembroId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}/foto")]
        [Authorize(Roles = "admin,gestor de miembros,auditor,lider")]
        public async Task<IActionResult> ObtenerFotoPerfil(int id)
        {
            try
            {
                var resultado = await _megaStorageService.ObtenerFotoPerfilAsync(id);

                if (resultado == null)
                {
                    return NotFound(new { message = "No se encontró foto de perfil para este miembro" });
                }

                var (contenido, extension) = resultado.Value;
                var contentType = extension.ToLower() switch
                {
                    "jpg" or "jpeg" => "image/jpeg",
                    "png" => "image/png",
                    _ => "application/octet-stream"
                };

                return File(contenido, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al obtener la foto de perfil",
                    error = ex.Message
                });
            }
        }

        [HttpPut("ActualizarMiembro/{id}")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<IActionResult> ActualizarMiembroCompleto(int id, [FromBody] MiembroDTO miembroDto)
        {
            if (id != miembroDto.id)
            {
                return BadRequest("El ID del miembro en la URL no coincide con el ID en el cuerpo de la solicitud.");
            }

            var miembroExistente = await _context.Miembros
                .Include(m => m.IdInformacionSaludNavigation)
                    .ThenInclude(s => s.IdAlergia)
                .Include(m => m.IdInformacionSaludNavigation)
                    .ThenInclude(s => s.IdCondicionMedicas)
                .Include(m => m.IdInformacionSaludNavigation)
                    .ThenInclude(s => s.IdMedicamentos)
                .Include(m => m.IdInformacionSaludNavigation)
                    .ThenInclude(s => s.TelefonosEmergencia)
                .Include(m => m.IdTrayectoriaNavigation)
                .Include(m => m.IdDireccionNavigation)
                .Include(m => m.IdInformacionEclesiasticaNavigation)
                    .ThenInclude(ie => ie.IdBautismoNavigation)
                .Include(m => m.IdInformacionEclesiasticaNavigation)
                    .ThenInclude(ie => ie.SeminariosCursados)
                        .ThenInclude(sc => sc.IdSeminariosNavigation)
                .Include(m => m.IdDatosPersonalesNavigation)
                    .ThenInclude(ip => ip.IdHijos)
                    .FirstOrDefaultAsync(m => m.IdMiembros == id);

            if (miembroExistente == null)
            {
                return NotFound($"Miembro con ID {id} no encontrado.");
            }

            // Info Básica
            if (miembroDto.info_basica_miembro != null)
            {
                var dniExistente = await _context.Miembros.AnyAsync(m => m.Dni == miembroDto.info_basica_miembro.dni && m.IdMiembros != id);
                if (dniExistente)
                    return BadRequest("El dni coincide con el de otro miembro");

                miembroExistente.Dni = miembroDto.info_basica_miembro.dni ?? miembroExistente.Dni;
                if (miembroDto.info_basica_miembro.fecha_nacimiento.HasValue)
                {
                    miembroExistente.FechaNacimiento = DateOnly.FromDateTime(miembroDto.info_basica_miembro.fecha_nacimiento.Value);
                }
                else
                {
                    miembroExistente.FechaNacimiento = miembroExistente.FechaNacimiento; 
                }
                miembroExistente.Nombre = miembroDto.info_basica_miembro.nombre ?? miembroExistente.Nombre;
                miembroExistente.Apellido = miembroDto.info_basica_miembro.apellido ?? miembroExistente.Apellido;
                miembroExistente.LugarNacimiento = miembroDto.info_basica_miembro.lugarNacimiento ?? miembroExistente.LugarNacimiento;
                miembroExistente.Nacionalidad = miembroDto.info_basica_miembro.nacionalidad ?? miembroExistente.Nacionalidad;
                miembroExistente.Telefono = miembroDto.info_basica_miembro.telefono ?? miembroExistente.Telefono;
                miembroExistente.TelefonoFijo = miembroDto.info_basica_miembro.telefono_fijo ?? miembroExistente.TelefonoFijo;
                miembroExistente.Sexo = miembroDto.info_basica_miembro.sexo;
            }

            // Información de Salud
            if (_miembroService.TieneInformacionSalud(miembroDto.info_salud))
            {
                miembroExistente.IdInformacionSaludNavigation ??= new InformacionSalud();
                miembroExistente.IdInformacionSaludNavigation.GrupoSanguineo = miembroDto.info_salud.grupo_sanguineo;
                miembroExistente.IdInformacionSaludNavigation.Observaciones = miembroDto.info_salud.observaciones;

                // Actualizar Alergias (
                miembroExistente.IdInformacionSaludNavigation.IdAlergia.Clear();
                foreach (var alergiaNombre in miembroDto.info_salud.alergias)
                {
                    var alergiaExistente = await _context.Alergias.FirstOrDefaultAsync(a => a.Nombre == alergiaNombre);
                    if (alergiaExistente != null)
                    {
                        miembroExistente.IdInformacionSaludNavigation.IdAlergia.Add(alergiaExistente);
                    }
                    else
                    {
                        var nuevaAlergia = new Alergia { Nombre = alergiaNombre };
                        _context.Alergias.Add(nuevaAlergia);
                        miembroExistente.IdInformacionSaludNavigation.IdAlergia.Add(nuevaAlergia);
                    }
                }

                // Actualizar Condiciones Médicas 
                miembroExistente.IdInformacionSaludNavigation.IdCondicionMedicas.Clear();
                foreach (var condicionNombre in miembroDto.info_salud.condiciones_medicas)
                {
                    var condicionExistente = await _context.CondicionesMedicas.FirstOrDefaultAsync(c => c.Condicion == condicionNombre);
                    if (condicionExistente != null)
                    {
                        miembroExistente.IdInformacionSaludNavigation.IdCondicionMedicas.Add(condicionExistente);
                    }
                    else
                    {
                        var nuevaCondicion = new CondicionMedica { Condicion = condicionNombre };
                        _context.CondicionesMedicas.Add(nuevaCondicion);
                        miembroExistente.IdInformacionSaludNavigation.IdCondicionMedicas.Add(nuevaCondicion);
                    }
                }

                // Actualizar Medicamentos 
                miembroExistente.IdInformacionSaludNavigation.IdMedicamentos.Clear();
                foreach (var medicamentoNombre in miembroDto.info_salud.medicamentos)
                {
                    var medicamentoExistente = await _context.Medicamentos.FirstOrDefaultAsync(m => m.Nombre == medicamentoNombre);
                    if (medicamentoExistente != null)
                    {
                        miembroExistente.IdInformacionSaludNavigation.IdMedicamentos.Add(medicamentoExistente);
                    }
                    else
                    {
                        var nuevoMedicamento = new Medicamento { Nombre = medicamentoNombre };
                        _context.Medicamentos.Add(nuevoMedicamento);
                        miembroExistente.IdInformacionSaludNavigation.IdMedicamentos.Add(nuevoMedicamento);
                    }
                }

                // Actualizar Teléfonos de Emergencia 
                _context.TelefonosEmergencias.RemoveRange(miembroExistente.IdInformacionSaludNavigation.TelefonosEmergencia);
                miembroExistente.IdInformacionSaludNavigation.TelefonosEmergencia.Clear();
                foreach (var telefonoDto in miembroDto.info_salud.telefonosEmergencias)
                {
                    miembroExistente.IdInformacionSaludNavigation.TelefonosEmergencia.Add(new TelefonoEmergencia
                    {
                        Propietario = telefonoDto.propietario_telefono,
                        Telefono = telefonoDto.nro_telefono,
                        IdInformacionSalud = miembroExistente.IdInformacionSaludNavigation.IdInformacionSalud
                    });
                }
            }

            // Información Académica
            if (_miembroService.TieneInformacionAcademica(miembroDto.info_academica))
            {
                miembroExistente.IdTrayectoriaNavigation ??= new Trayectoria();
                miembroExistente.IdTrayectoriaNavigation.EstudiosPrimario = miembroDto.info_academica.estudios_primario;
                miembroExistente.IdTrayectoriaNavigation.EstudiosSecundario = miembroDto.info_academica.estudios_secundario;
                miembroExistente.IdTrayectoriaNavigation.EstudiosTerciario = miembroDto.info_academica.estudios_terciario;
                miembroExistente.IdTrayectoriaNavigation.EstudiosUniversitario = miembroDto.info_academica.estudios_universitario;
                miembroExistente.IdTrayectoriaNavigation.Carrera = miembroDto.info_academica.carrera;
                miembroExistente.IdTrayectoriaNavigation.CursosRealizados = miembroDto.info_academica.cursos_realizados;
            }

            // Información Laboral
            if (_miembroService.TieneInformacionLaboral(miembroDto.info_laboral))
            {
                miembroExistente.IdTrayectoriaNavigation ??= new Trayectoria();
                miembroExistente.IdTrayectoriaNavigation.SituacionLaboral = miembroDto.info_laboral.situacion_laboral;
                miembroExistente.IdTrayectoriaNavigation.Rubro = miembroDto.info_laboral.rubro;
            }

            // Dirección
            if (_miembroService.TieneDireccion(miembroDto.direccion))
            {
                miembroExistente.IdDireccionNavigation ??= new Direccion();
                miembroExistente.IdDireccionNavigation.Calle = miembroDto.direccion.calle;
                miembroExistente.IdDireccionNavigation.Altura = miembroDto.direccion.altura;
                miembroExistente.IdDireccionNavigation.Barrio = miembroDto.direccion.barrio;
            }

            // Información Eclesiástica
            if (_miembroService.TieneInformacionEclesiastica(miembroDto.info_eclesiastica))
            {
                miembroExistente.IdInformacionEclesiasticaNavigation ??= new InformacionEclesiastica();
                miembroExistente.IdInformacionEclesiasticaNavigation.Convocante = miembroDto.info_eclesiastica.convocante;
                miembroExistente.IdInformacionEclesiasticaNavigation.FechaAsiste = miembroDto.info_eclesiastica.fecha_desde_la_que_asiste;

                // Actualizar Bautismo
                if (miembroDto.info_eclesiastica.realizo_bautismo == true)
                {
                    miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation ??= new Bautismo();
                    miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Realizo = miembroDto.info_eclesiastica.realizo_bautismo;
                    miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Pastor = miembroDto.info_eclesiastica.pastor;
                    miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Fecha = miembroDto.info_eclesiastica.fecha_de_bautismo;
                    miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Lugar = miembroDto.info_eclesiastica.lugar_bautismo;
                }
                else if (miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation != null)
                {
                    _context.Bautismos.Remove(miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation);
                    miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation = null;
                    miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismos = null;
                }

                // Actualizar Seminarios Cursados
                _context.SeminariosCursados.RemoveRange(miembroExistente.IdInformacionEclesiasticaNavigation.SeminariosCursados);
                miembroExistente.IdInformacionEclesiasticaNavigation.SeminariosCursados.Clear();
                foreach (var seminarioDto in miembroDto.info_eclesiastica.seminarios)
                {
                    var seminarioExistente = await _context.Seminarios
                        .FirstOrDefaultAsync(s => s.Nombre.ToLower() == seminarioDto.nombre.ToLower());

                    if (seminarioExistente == null)
                    {
                        seminarioExistente = new Seminario
                        {
                            Nombre = seminarioDto.nombre,
                            Activo = true,
                            AnioComienzo = seminarioDto.anio_comienzo
                        };
                        _context.Seminarios.Add(seminarioExistente);
                    }

                    miembroExistente.IdInformacionEclesiasticaNavigation.SeminariosCursados.Add(new SeminariosCursado
                    {
                        IdSeminario = seminarioExistente.IdSeminario,
                        IdSeminariosNavigation = seminarioExistente,
                        Estado = seminarioDto.estado ?? "Cursado",
                        AnioCursado = seminarioDto.anio_cursado ?? DateTime.Now.Year
                    });
                }
            }

            // Información Personal
            if (_miembroService.TieneInformacionPersonal(miembroDto.info_personal))
            {
                miembroExistente.IdDatosPersonalesNavigation ??= new DatoPersonal();
                miembroExistente.IdDatosPersonalesNavigation.EstadoCivil = miembroDto.info_personal.estado_civil;
                miembroExistente.IdDatosPersonalesNavigation.Pareja = miembroDto.info_personal.pareja;
                miembroExistente.IdDatosPersonalesNavigation.TelefonoAlternativo = miembroDto.info_personal.telefono_alternativo;

                miembroExistente.IdDatosPersonalesNavigation.IdHijos.Clear();
                foreach (var hijoDto in miembroDto.info_personal.hijos)
                {
                    var hijoExistente = await _context.Hijos.FirstOrDefaultAsync(h => h.Nombre == hijoDto.nombre && h.Apellido == hijoDto.apellido);
                    if (hijoExistente != null)
                    {
                        miembroExistente.IdDatosPersonalesNavigation.IdHijos.Add(hijoExistente);
                    }
                    else
                    {
                        var nuevoHijo = new Hijo { Nombre = hijoDto.nombre, Apellido = hijoDto.apellido };
                        _context.Hijos.Add(nuevoHijo);
                        miembroExistente.IdDatosPersonalesNavigation.IdHijos.Add(nuevoHijo);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return NoContent(); 
        }

        [HttpPatch("ActualizarMiembro/{id}/seccion/{section}")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<IActionResult> ActualizarMiembroSeccion(int id, string section, [FromBody] JsonElement patchData)
        {
            var miembroExistente = await _context.Miembros
                .Include(m => m.IdInformacionSaludNavigation)
                    .ThenInclude(s => s.IdAlergia)
                .Include(m => m.IdInformacionSaludNavigation)
                    .ThenInclude(s => s.IdCondicionMedicas)
                .Include(m => m.IdInformacionSaludNavigation)
                    .ThenInclude(s => s.IdMedicamentos)
                .Include(m => m.IdInformacionSaludNavigation)
                    .ThenInclude(s => s.TelefonosEmergencia)
                .Include(m => m.IdTrayectoriaNavigation)
                .Include(m => m.IdDireccionNavigation)
                .Include(m => m.IdInformacionEclesiasticaNavigation)
                    .ThenInclude(ie => ie.IdBautismoNavigation)
                .Include(m => m.IdInformacionEclesiasticaNavigation)
                    .ThenInclude(ie => ie.SeminariosCursados)
                        .ThenInclude(sc => sc.IdSeminariosNavigation)
                .Include(m => m.IdDatosPersonalesNavigation)
                    .ThenInclude(ip => ip.IdHijos)
                    .FirstOrDefaultAsync(m => m.IdMiembros == id);

            if (miembroExistente == null)
            {
                return NotFound($"Miembro con ID {id} no encontrado.");
            }

            try
            {
                switch (section.ToLower()) 
                {
                    case "basica":
                        var infoBasicaPatch = patchData.Deserialize<InfoBasicaMiembroDTO>();
                        if (infoBasicaPatch == null) return BadRequest("Datos de sección básica inválidos.");

                        // Validar que los campos requeridos no sean null
                        if (string.IsNullOrWhiteSpace(infoBasicaPatch.dni))
                            return BadRequest("El DNI es obligatorio");
                        if (string.IsNullOrWhiteSpace(infoBasicaPatch.nombre))
                            return BadRequest("El nombre es obligatorio");
                        if (string.IsNullOrWhiteSpace(infoBasicaPatch.apellido))
                            return BadRequest("El apellido es obligatorio");
                        if (string.IsNullOrWhiteSpace(infoBasicaPatch.telefono))
                            return BadRequest("El teléfono es obligatorio");
                        if (string.IsNullOrWhiteSpace(infoBasicaPatch.nacionalidad))
                            return BadRequest("La nacionalidad es obligatoria");
                        if (!infoBasicaPatch.fecha_nacimiento.HasValue)
                            return BadRequest("La fecha de nacimiento es obligatoria");

                        // Verificar que el DNI no exista en otro miembro
                        var dniExistente = await _context.Miembros
                            .AnyAsync(m => m.Dni == infoBasicaPatch.dni && m.IdMiembros != id);
                        if (dniExistente)
                            return BadRequest("El DNI coincide con el de otro miembro");

                        miembroExistente.Dni = infoBasicaPatch.dni;
                        miembroExistente.FechaNacimiento = DateOnly.FromDateTime(infoBasicaPatch.fecha_nacimiento.Value);
                        miembroExistente.Nombre = infoBasicaPatch.nombre;
                        miembroExistente.Apellido = infoBasicaPatch.apellido;
                        miembroExistente.LugarNacimiento = infoBasicaPatch.lugarNacimiento ?? miembroExistente.LugarNacimiento;
                        miembroExistente.Nacionalidad = infoBasicaPatch.nacionalidad;
                        miembroExistente.Telefono = infoBasicaPatch.telefono;
                        miembroExistente.TelefonoFijo = infoBasicaPatch.telefono_fijo ?? miembroExistente.TelefonoFijo;
                        miembroExistente.Sexo = infoBasicaPatch.sexo;
                        break;

                    case "salud":
                        miembroExistente.IdInformacionSaludNavigation ??= new InformacionSalud();
                        var infoSaludPatch = patchData.Deserialize<InformacionSaludDTO>();
                        if (infoSaludPatch == null) return BadRequest("Datos de sección de salud inválidos.");

                        miembroExistente.IdInformacionSaludNavigation.GrupoSanguineo = infoSaludPatch.grupo_sanguineo ?? miembroExistente.IdInformacionSaludNavigation.GrupoSanguineo;
                        miembroExistente.IdInformacionSaludNavigation.Observaciones = infoSaludPatch.observaciones ?? miembroExistente.IdInformacionSaludNavigation.Observaciones;

                        miembroExistente.IdInformacionSaludNavigation.IdAlergia.Clear(); 
                        foreach (var alergiaNombre in infoSaludPatch.alergias ?? new List<string>())
                        {
                            var alergiaExistente = await _context.Alergias.FirstOrDefaultAsync(a => a.Nombre == alergiaNombre);
                            if (alergiaExistente != null)
                            {
                                miembroExistente.IdInformacionSaludNavigation.IdAlergia.Add(alergiaExistente);
                            }
                            else
                            {
                                var nuevaAlergia = new Alergia { Nombre = alergiaNombre };
                                _context.Alergias.Add(nuevaAlergia);
                                miembroExistente.IdInformacionSaludNavigation.IdAlergia.Add(nuevaAlergia);
                            }
                        }

                        miembroExistente.IdInformacionSaludNavigation.IdCondicionMedicas.Clear();
                        foreach (var condicionNombre in infoSaludPatch.condiciones_medicas ?? new List<string>())
                        {
                            var condicionExistente = await _context.CondicionesMedicas.FirstOrDefaultAsync(c => c.Condicion == condicionNombre);
                            if (condicionExistente != null)
                            {
                                miembroExistente.IdInformacionSaludNavigation.IdCondicionMedicas.Add(condicionExistente);
                            }
                            else
                            {
                                var nuevaCondicion = new CondicionMedica { Condicion = condicionNombre };
                                _context.CondicionesMedicas.Add(nuevaCondicion);
                                miembroExistente.IdInformacionSaludNavigation.IdCondicionMedicas.Add(nuevaCondicion);
                            }
                        }

                        miembroExistente.IdInformacionSaludNavigation.IdMedicamentos.Clear();
                        foreach (var medicamentoNombre in infoSaludPatch.medicamentos ?? new List<string>())
                        {
                            var medicamentoExistente = await _context.Medicamentos.FirstOrDefaultAsync(m => m.Nombre == medicamentoNombre);
                            if (medicamentoExistente != null)
                            {
                                miembroExistente.IdInformacionSaludNavigation.IdMedicamentos.Add(medicamentoExistente);
                            }
                            else
                            {
                                var nuevoMedicamento = new Medicamento { Nombre = medicamentoNombre };
                                _context.Medicamentos.Add(nuevoMedicamento);
                                miembroExistente.IdInformacionSaludNavigation.IdMedicamentos.Add(nuevoMedicamento);
                            }
                        }

                        _context.TelefonosEmergencias.RemoveRange(miembroExistente.IdInformacionSaludNavigation.TelefonosEmergencia);
                        miembroExistente.IdInformacionSaludNavigation.TelefonosEmergencia.Clear(); 

                        foreach (var telefonoDto in infoSaludPatch.telefonosEmergencias ?? new List<TelefonoEmergenciaDTO>())
                        {
                            miembroExistente.IdInformacionSaludNavigation.TelefonosEmergencia.Add(new TelefonoEmergencia
                            {
                                Propietario = telefonoDto.propietario_telefono,
                                Telefono = telefonoDto.nro_telefono,
                                IdInformacionSalud = miembroExistente.IdInformacionSaludNavigation.IdInformacionSalud
                            });
                        }
                        break;

                    case "academica":
                        miembroExistente.IdTrayectoriaNavigation ??= new Trayectoria();
                        var infoAcademicaPatch = patchData.Deserialize<InformacionAcademicaDTO>();
                        if (infoAcademicaPatch == null) return BadRequest("Datos de sección académica inválidos.");
                        miembroExistente.IdTrayectoriaNavigation.EstudiosPrimario = infoAcademicaPatch.estudios_primario ?? miembroExistente.IdTrayectoriaNavigation.EstudiosPrimario;
                        miembroExistente.IdTrayectoriaNavigation.EstudiosSecundario = infoAcademicaPatch.estudios_secundario ?? miembroExistente.IdTrayectoriaNavigation.EstudiosSecundario;
                        miembroExistente.IdTrayectoriaNavigation.EstudiosTerciario = infoAcademicaPatch.estudios_terciario ?? miembroExistente.IdTrayectoriaNavigation.EstudiosTerciario;
                        miembroExistente.IdTrayectoriaNavigation.EstudiosUniversitario = infoAcademicaPatch.estudios_universitario ?? miembroExistente.IdTrayectoriaNavigation.EstudiosUniversitario;
                        miembroExistente.IdTrayectoriaNavigation.Carrera = infoAcademicaPatch.carrera ?? miembroExistente.IdTrayectoriaNavigation.Carrera;
                        miembroExistente.IdTrayectoriaNavigation.CursosRealizados = infoAcademicaPatch.cursos_realizados ?? miembroExistente.IdTrayectoriaNavigation.CursosRealizados;
                        break;

                    case "laboral":
                        miembroExistente.IdTrayectoriaNavigation ??= new Trayectoria();
                        var infoLaboralPatch = patchData.Deserialize<InformacionLaboralDTO>();
                        if (infoLaboralPatch == null) return BadRequest("Datos de sección laboral inválidos.");
                        miembroExistente.IdTrayectoriaNavigation.SituacionLaboral = infoLaboralPatch.situacion_laboral ?? miembroExistente.IdTrayectoriaNavigation.SituacionLaboral;
                        miembroExistente.IdTrayectoriaNavigation.Rubro = infoLaboralPatch.rubro ?? miembroExistente.IdTrayectoriaNavigation.Rubro;
                        break;

                    case "direccion":
                        miembroExistente.IdDireccionNavigation ??= new Direccion();
                        var direccionPatch = patchData.Deserialize<DireccionDTO>();
                        if (direccionPatch == null) return BadRequest("Datos de sección de dirección inválidos.");
                        miembroExistente.IdDireccionNavigation.Calle = direccionPatch.calle ?? miembroExistente.IdDireccionNavigation.Calle;
                        miembroExistente.IdDireccionNavigation.Altura = direccionPatch.altura ?? miembroExistente.IdDireccionNavigation.Altura;
                        miembroExistente.IdDireccionNavigation.Barrio = direccionPatch.barrio ?? miembroExistente.IdDireccionNavigation.Barrio;
                        break;

                    case "eclesiastica":
                        miembroExistente.IdInformacionEclesiasticaNavigation ??= new InformacionEclesiastica();
                        var infoEclesiasticaPatch = patchData.Deserialize<InformacionEclesiasticaDTO>();
                        if (infoEclesiasticaPatch == null) return BadRequest("Datos de sección eclesiástica inválidos.");

                        miembroExistente.IdInformacionEclesiasticaNavigation.Convocante = infoEclesiasticaPatch.convocante ?? miembroExistente.IdInformacionEclesiasticaNavigation.Convocante;
                        miembroExistente.IdInformacionEclesiasticaNavigation.FechaAsiste = infoEclesiasticaPatch.fecha_desde_la_que_asiste ?? miembroExistente.IdInformacionEclesiasticaNavigation.FechaAsiste;
                       
                        if (infoEclesiasticaPatch.realizo_bautismo == true)
                        {
                            miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation ??= new Bautismo();
                            miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Realizo = infoEclesiasticaPatch.realizo_bautismo ?? miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Realizo;
                            miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Pastor = infoEclesiasticaPatch.pastor ?? miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Pastor;
                            miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Fecha = infoEclesiasticaPatch.fecha_de_bautismo ?? miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Fecha;
                            miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Lugar = infoEclesiasticaPatch.lugar_bautismo ?? miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation.Lugar;
                        }
                        else if (miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation != null)
                        {
                            _context.Bautismos.Remove(miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation);
                            miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismoNavigation = null;
                            miembroExistente.IdInformacionEclesiasticaNavigation.IdBautismos = null;
                        }

                        _context.SeminariosCursados.RemoveRange(miembroExistente.IdInformacionEclesiasticaNavigation.SeminariosCursados);
                        miembroExistente.IdInformacionEclesiasticaNavigation.SeminariosCursados.Clear();

                        foreach (var seminarioDto in infoEclesiasticaPatch.seminarios ?? new List<SeminarioDTO>())
                        {
                            var seminarioExistente = await _context.Seminarios
                                .FirstOrDefaultAsync(s => s.Nombre.ToLower() == seminarioDto.nombre.ToLower());

                            if (seminarioExistente == null)
                            {
                                seminarioExistente = new Seminario
                                {
                                    Nombre = seminarioDto.nombre,
                                    Activo = true, 
                                    AnioComienzo = seminarioDto.anio_comienzo
                                };
                                _context.Seminarios.Add(seminarioExistente);
                            }

                            miembroExistente.IdInformacionEclesiasticaNavigation.SeminariosCursados.Add(new SeminariosCursado
                            {
                                IdSeminario = seminarioExistente.IdSeminario,
                                IdSeminariosNavigation = seminarioExistente, 
                                Estado = seminarioDto.estado ?? "Cursado",
                                AnioCursado = seminarioDto.anio_cursado ?? DateTime.Now.Year
                            });
                        }
                        break;

                    case "personal":
                        miembroExistente.IdDatosPersonalesNavigation ??= new DatoPersonal();
                        var infoPersonalPatch = patchData.Deserialize<InformacionPersonalDTO>();
                        if (infoPersonalPatch == null) return BadRequest("Datos de sección personal inválidos.");

                        miembroExistente.IdDatosPersonalesNavigation.EstadoCivil = infoPersonalPatch.estado_civil ?? miembroExistente.IdDatosPersonalesNavigation.EstadoCivil;
                        miembroExistente.IdDatosPersonalesNavigation.Pareja = infoPersonalPatch.pareja ?? miembroExistente.IdDatosPersonalesNavigation.Pareja;
                        miembroExistente.IdDatosPersonalesNavigation.TelefonoAlternativo = infoPersonalPatch.telefono_alternativo ?? miembroExistente.IdDatosPersonalesNavigation.TelefonoAlternativo;

                        miembroExistente.IdDatosPersonalesNavigation.IdHijos.Clear();
                        foreach (var hijoDto in infoPersonalPatch.hijos ?? new List<HijoDTO>())
                        {
                            var hijoExistente = await _context.Hijos.FirstOrDefaultAsync(h => h.Nombre == hijoDto.nombre && h.Apellido == hijoDto.apellido);
                            if (hijoExistente != null)
                            {
                                miembroExistente.IdDatosPersonalesNavigation.IdHijos.Add(hijoExistente);
                            }
                            else
                            {
                                var nuevoHijo = new Hijo { Nombre = hijoDto.nombre, Apellido = hijoDto.apellido };
                                _context.Hijos.Add(nuevoHijo);
                                miembroExistente.IdDatosPersonalesNavigation.IdHijos.Add(nuevoHijo);
                            }
                        }
                        break;  
                    default:
                        return BadRequest($"Sección '{section}' no válida para actualización.");
                }
                await _context.SaveChangesAsync();

                return NoContent(); 
            }
            catch (JsonException ex)
            {
                return BadRequest($"Error al deserializar los datos JSON para la sección '{section}': {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor al actualizar la sección '{section}': {ex.Message}");
            }
        }

        [HttpDelete("EliminarMiembro/{miembroId}")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<IActionResult> EliminarMiembro(int miembroId)
        {
            try
            {
                var miembro = await _context.Miembros
                .Include(m => m.IdInformacionSaludNavigation)
                .Include(m => m.IdTrayectoriaNavigation)
                .Include(m => m.IdDireccionNavigation)
                .Include(m => m.IdInformacionEclesiasticaNavigation)
                .Include(m => m.IdDatosPersonalesNavigation)
                .FirstOrDefaultAsync(m => m.IdMiembros == miembroId);
                if (miembro == null)
                {
                    return NotFound($"Miembro con ID {miembroId} no encontrado.");
                }

                var tieneUsuario = await _context.Usuarios.AnyAsync(u => u.IdMiembros == miembroId);
                var esLider = await _context.Lideres.AnyAsync(l => l.IdMiembros == miembroId);

                if (esLider && tieneUsuario)
                {
                    var grupoActivo = await _context.PerteneceGrupos
                        .Include(pg => pg.IdGruposNavigation)
                        .FirstOrDefaultAsync(pg => pg.IdMiembros == miembroId && pg.Ocupacion == "Líder");

                    if (grupoActivo != null)
                    {
                        return BadRequest(new 
                        { 
                            message = $"No se puede eliminar el miembro '{miembro.Nombre} {miembro.Apellido}' porque está vinculado como líder al grupo '{grupoActivo.IdGruposNavigation?.Nombre}'. Primero debe desvincularse del grupo." 
                        });
                    }
                }

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdMiembros == miembroId);
                if (usuario != null)
                {
                    usuario.IdMiembros = null;
                    usuario.IdMiembrosNavigation = null;
                }
                if(miembro.IdInformacionEclesiasticaNavigation != null)
                {
                    _context.InformacionesEclesiasticas.Remove(miembro.IdInformacionEclesiasticaNavigation);
                }
                if(miembro.IdInformacionSaludNavigation != null)
                {
                    _context.InformacionesSalud.Remove(miembro.IdInformacionSaludNavigation);
                }
                if (miembro.IdTrayectoriaNavigation != null)
                {
                    _context.Trayectorias.Remove(miembro.IdTrayectoriaNavigation);
                }
                if (miembro.IdDatosPersonalesNavigation != null)
                {
                    _context.DatosPersonales.Remove(miembro.IdDatosPersonalesNavigation);
                }
                if (miembro.IdDireccionNavigation != null)
                {
                    _context.Direcciones.Remove(miembro.IdDireccionNavigation);
                }
                _context.Miembros.Remove(miembro);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el miembro con ID {MiembroId}", miembroId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}/foto")]
        [Authorize(Roles = "admin,gestor de miembros")]
        public async Task<IActionResult> EliminarFotoPerfil(int id)
        {
            try
            {
                var eliminado = await _megaStorageService.EliminarFotoPerfilAsync(id);

                if (!eliminado)
                {
                    return NotFound(new { message = "No se encontro foto de perfil para eliminar" });
                }

                return Ok(new { message = "Foto de perfil eliminada correctamente" });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Error al eliminar la foto de perfil",
                    error = ex.Message
                });
            }
        }
    }
}