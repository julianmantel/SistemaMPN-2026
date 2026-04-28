using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SistemaMPN.Modules.Archivos.Services;
using System.Security.Claims;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Tesoreria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoController : ControllerBase
    {
        private readonly ILogger<DocumentoController> _logger;
        private readonly DataContext _context;
        private readonly IMegaStorageService _megaStorageService;
        public DocumentoController(ILogger<DocumentoController> logger, DataContext context, IMegaStorageService megaStorageService)
        {
            _logger = logger;
            _context = context;
            _megaStorageService = megaStorageService;
        }

        [HttpPost("GuardarDocumento")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> GuardarDocumento(
            [FromForm] DocumentoDTO documentoDto,
            [FromForm(Name = "archivo")] IFormFile? archivo)
        {
            try
            {
                var tesorero = await _context.Tesoreros.FindAsync(documentoDto.id_tesorero);
                if (tesorero == null)
                {
                    return BadRequest("El tesorero especificado no existe");
                }

                var supervisor = await _context.Tesoreros.FindAsync(documentoDto.id_supervisor);
                if (supervisor == null)
                {
                    return BadRequest("El supervisor especificado no existe");
                }

                // Crear el documento
                var documento = new Documento
                {
                    NroDocumento = documentoDto.nro_documento,
                    Fecha = documentoDto.fecha,
                    Tipo = documentoDto.tipo,
                    Firmado = documentoDto.firmado,
                    DocumentoTesoreros = new List<DocumentoTesorero>()
                };

                // Agregar el creador (tesorero)
                documento.DocumentoTesoreros.Add(new DocumentoTesorero
                {
                    IdTesorero = documentoDto.id_tesorero,
                    EsCreador = true,
                    EsSupervisor = false
                });
                // Agregar el supervisor
                documento.DocumentoTesoreros.Add(new DocumentoTesorero
                {
                    IdTesorero = documentoDto.id_supervisor ?? documentoDto.id_tesorero,
                    EsCreador = false,
                    EsSupervisor = true
                });

                _context.Documentos.Add(documento);
                await _context.SaveChangesAsync();

                string nombreArchivo = null;

                if (archivo != null && archivo.Length > 0)
                {
                    try
                    {
                        nombreArchivo = await SubirDocumentoMega(
                            documento.IdDocumento,
                            documentoDto.tipo,
                            documentoDto.firmado,
                            archivo);

                        if(documentoDto.firmado)
                        {
                            documento.NombreArchivoFirmado = nombreArchivo;
                        }
                        else
                        {
                            documento.NombreArchivoOriginal = nombreArchivo;
                        }
                        await _context.SaveChangesAsync();
                    }
                    catch(Exception exMega)
                    {
                        _logger.LogWarning(exMega, "No se pudo subir el archivo a MEGA después de guardar el documento en la base de datos.");
                        return Ok(new
                        {
                            message = "Documento guardado exitosamente pero no subido a MEGA",
                            documentoId = documento.IdDocumento,
                            advertencia = exMega.Message
                        });
                    }
                }

                return Ok(new{
                    message = "Documento guardado exitosamente",
                    documentoId = documento.IdDocumento,
                    nombreArchivo = nombreArchivo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el documento");
                return StatusCode(500, $"Error al guardar: {ex.Message}");
            }
        }

        //Aux para MEGA
        private async Task<string> SubirDocumentoMega(int documentoId, string tipoDocumento, bool esFirmado, IFormFile archivo)
        {
            if(archivo == null || archivo.Length == 0)
            {
                throw new ArgumentException("No se ha proporcionado ningún archivo.");
            }

            var extension = Path.GetExtension(archivo.FileName).ToLower();
            if(extension != ".pdf")
            {
                throw new ArgumentException("Solo se permiten archivos PDF.");
            }

            if(archivo.Length > 10 * 1024 * 1024)
            {
                throw new ArgumentException("El archivo PDF es demasiado grande. Tamaño máximo: 10MB");
            }

            using var stream = archivo.OpenReadStream();
            return await _megaStorageService.SubirDocumentoTesoreriaAsync(documentoId, tipoDocumento
                , esFirmado, stream);
        }

        [HttpPut("SubirDocumentoFirmado")]
        [Authorize(Roles = "protesorero, tesorero, admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubirDocumentoFirmado(
        [FromForm] int documentoId,
        [FromForm] string tipoDocumento,
        [FromForm] bool esFirmado,
        [FromForm(Name = "archivo")] IFormFile archivo)
        {
            try
            {
                // Validaciones básicas
                if (archivo == null || archivo.Length == 0)
                {
                    return BadRequest("No se ha proporcionado ningún archivo.");
                }
                if (string.IsNullOrWhiteSpace(tipoDocumento))
                {
                    return BadRequest("Falta tipo de documento.");
                }
                var extension = Path.GetExtension(archivo.FileName).TrimStart('.').ToLower();
                if (extension != "pdf")
                {
                    return BadRequest("Solo se permiten archivos PDF.");
                }
                if (archivo.Length > 10 * 1024 * 1024)
                {
                    return BadRequest("El archivo PDF es demasiado grande. Tamaño máximo: 10MB" );
                }

                // Buscar el documento en la BD
                var documento = await _context.Documentos.FindAsync(documentoId);
                if (documento == null)
                {
                    return NotFound("Documento no encontrado.");
                }

                // Subir el archivo a MEGA
                using var stream = archivo.OpenReadStream();
                var nombreArchivo = await _megaStorageService.SubirDocumentoTesoreriaAsync(
                    documentoId,
                    tipoDocumento,
                    esFirmado,
                    stream);

                // Actualizar la BD
                documento.Firmado = true;
                documento.NombreArchivoFirmado = nombreArchivo;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Documento firmado subido y actualizado exitosamente",
                    nombreArchivo = nombreArchivo,
                    documentoId = documentoId,
                    tipoDocumento = tipoDocumento,
                    esFirmado = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir el documento firmado");
                return StatusCode(500, new { message = "Error al subir el documento", error = ex.Message });
            }
        }

        [HttpGet("GetTodosLosDocumentos")]
        [Authorize(Roles = "protesorero, tesorero, admin, auditor")]
        public async Task<ActionResult<List<DocumentoDTO>>> GetTodosLosDocumentos()
        {
            try
            {
                var documentos = await _context.Documentos
                    .Select(d => new DocumentoDTO
                    {
                        id_documento = d.IdDocumento,
                        nro_documento = d.NroDocumento,
                        fecha = d.Fecha,
                        tipo = d.Tipo,
                        firmado = d.Firmado,

                        // Creador
                        id_tesorero = d.DocumentoTesoreros
                        .Where(t => t.EsCreador)
                        .Select(t => t.IdTesoreroNavigation.IdMiembros)
                        .FirstOrDefault(),

                        nombre_tesorero = d.DocumentoTesoreros
                        .Where(t => t.EsCreador)
                        .Select(t => $"{t.IdTesoreroNavigation.IdMiembrosNavigation.Nombre} {t.IdTesoreroNavigation.IdMiembrosNavigation.Apellido}")
                        .FirstOrDefault(),

                        cargo_tesorero = d.DocumentoTesoreros
                        .Where(t => t.EsCreador)
                        .Select(t => t.IdTesoreroNavigation.IsPro ? "Protesorero" : "Tesorero")
                        .FirstOrDefault(),

                        // Supervisor
                        id_supervisor = d.DocumentoTesoreros
                        .Where(t => t.EsSupervisor)
                        .Select(t => t.IdTesoreroNavigation.IdMiembros)
                        .FirstOrDefault(),

                        nombre_supervisor = d.DocumentoTesoreros
                        .Where(t => t.EsSupervisor)
                        .Select(t => $"{t.IdTesoreroNavigation.IdMiembrosNavigation.Nombre} {t.IdTesoreroNavigation.IdMiembrosNavigation.Apellido}")
                        .FirstOrDefault() ?? "Sin supervisor",

                        cargo_supervisor = d.DocumentoTesoreros
                        .Where(t => t.EsSupervisor)
                        .Select(t => t.IdTesoreroNavigation.IsPro ? "Protesorero" : "Tesorero")
                        .FirstOrDefault()
                    })
                    .OrderByDescending(d => d.fecha)
                    .ToListAsync();

                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los documentos");
                return StatusCode(500, "Error al obtener documentos");
            }
        }

        [HttpGet("GetDocumentosPorTesorero/{idMiembro}")]
        [Authorize(Roles = "protesorero, tesorero, admin, auditor")]
        public async Task<ActionResult<List<DocumentoDTO>>> GetDocumentosPorTesorero(int idMiembro)
        {
            try
            {
                var documentos = await _context.Documentos
                    .Where(d => d.DocumentoTesoreros.Any(dt => dt.IdTesoreroNavigation.IdMiembros == idMiembro))
                    .Select(d => new DocumentoDTO
                    {
                        id_documento = d.IdDocumento,
                        nro_documento = d.NroDocumento,
                        fecha = d.Fecha,
                        tipo = d.Tipo,
                        firmado = d.Firmado,

                        // --- Datos del Tesorero (el que consulta) ---
                        id_tesorero = d.DocumentoTesoreros
                        .Where(t => t.EsCreador)
                        .Select(t => t.IdTesoreroNavigation.IdMiembros)
                        .FirstOrDefault(),

                        nombre_tesorero = d.DocumentoTesoreros
                        .Where(t => t.EsCreador)
                        .Select(t => $"{t.IdTesoreroNavigation.IdMiembrosNavigation.Nombre} {t.IdTesoreroNavigation.IdMiembrosNavigation.Apellido}")
                        .FirstOrDefault(),

                        cargo_tesorero = d.DocumentoTesoreros
                        .Where(t => t.EsCreador)
                        .Select(t => t.IdTesoreroNavigation.IsPro ? "Protesorero" : "Tesorero")
                        .FirstOrDefault(),

                        // --- Datos del Supervisor (la otra persona en el documento) ---
                        id_supervisor = d.DocumentoTesoreros
                        .Where(t => t.EsSupervisor)
                        .Select(t => t.IdTesoreroNavigation.IdMiembros)
                        .FirstOrDefault(),

                        nombre_supervisor = d.DocumentoTesoreros
                        .Where(t => t.EsSupervisor)
                        .Select(t => $"{t.IdTesoreroNavigation.IdMiembrosNavigation.Nombre} {t.IdTesoreroNavigation.IdMiembrosNavigation.Apellido}")
                        .FirstOrDefault() ?? "Sin supervisor",

                        cargo_supervisor = d.DocumentoTesoreros
                        .Where(t => t.EsSupervisor)
                        .Select(t => t.IdTesoreroNavigation.IsPro ? "Protesorero" : "Tesorero")
                        .FirstOrDefault()
                    })
                    .OrderByDescending(d => d.fecha)
                    .ToListAsync();

                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos del tesorero");
                return StatusCode(500, "Error al obtener documentos");
            }
        }

        // Ver documento en navegador (inline)
        [HttpGet("VerDocumento/{documentoId}")]
        [Authorize(Roles = "protesorero, tesorero, admin, auditor")]
        public async Task<IActionResult> VerDocumento(int documentoId)
        {
            try
            {
                var documento = await _context.Documentos.FindAsync(documentoId);
                if (documento == null)
                {
                    return NotFound("Documento no encontrado.");
                }

                var bytes = await _megaStorageService.ObtenerDocumentoTesoreriaAsync(
                    documentoId,
                    documento.Tipo,
                    documento.Firmado);

                if (bytes == null || bytes.Length == 0)
                {
                    return NotFound("El archivo no existe en el almacenamiento.");
                }

                var nombreArchivo = documento.Firmado ? documento.NombreArchivoFirmado : documento.NombreArchivoOriginal;
                if (string.IsNullOrEmpty(nombreArchivo))
                {
                    nombreArchivo = $"{documento.Tipo}_{documentoId}{(documento.Firmado ? "_firmado" : "")}.pdf";
                }

                Response.Headers.Add("Content-Disposition", $"inline; filename=\"{nombreArchivo}\"");
                return File(bytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al visualizar el documento {DocumentoId}", documentoId);
                return StatusCode(500, new { message = "Error al obtener el documento", error = ex.Message });
            }
        }

        // Descargar documento (attachment)
        [HttpGet("DescargarDocumento/{documentoId}")]
        [Authorize(Roles = "protesorero, tesorero, admin, auditor")]
        public async Task<IActionResult> DescargarDocumento(int documentoId)
        {
            try
            {
                var documento = await _context.Documentos.FindAsync(documentoId);
                if (documento == null)
                {
                    return NotFound("Documento no encontrado.");
                }

                var bytes = await _megaStorageService.ObtenerDocumentoTesoreriaAsync(
                    documentoId,
                    documento.Tipo,
                    documento.Firmado);

                if (bytes == null || bytes.Length == 0)
                {
                    return NotFound("El archivo no existe en el almacenamiento.");
                }

                var nombreArchivo = documento.Firmado ? documento.NombreArchivoFirmado : documento.NombreArchivoOriginal;
                if (string.IsNullOrEmpty(nombreArchivo))
                {
                    nombreArchivo = $"{documento.Tipo}_{documentoId}{(documento.Firmado ? "_firmado" : "")}.pdf";
                }

                return File(bytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar el documento {DocumentoId}", documentoId);
                return StatusCode(500, new { message = "Error al descargar el documento", error = ex.Message });
            }
        }

        // Eliminar documento
        [HttpDelete("EliminarDocumento/{documentoId}")]
        [Authorize(Roles = "protesorero, tesorero, admin")]
        public async Task<IActionResult> EliminarDocumento(int documentoId)
        {
            try
            {
                var documento = await _context.Documentos
                    .Include(d => d.DocumentoTesoreros)
                    .FirstOrDefaultAsync(d => d.IdDocumento == documentoId);

                if (documento == null)
                {
                    return NotFound("Documento no encontrado.");
                }

                // ✅ Validar que NO esté firmado
                if (documento.Firmado)
                {
                    return BadRequest("No se puede eliminar un documento firmado.");
                }

                // ✅ Eliminar archivo de MEGA si existe
                if (!string.IsNullOrEmpty(documento.NombreArchivoOriginal))
                {
                    try
                    {
                        await _megaStorageService.EliminarDocumentoTesoreriaAsync(
                            documentoId,
                            documento.Tipo,
                            false);
                    }
                    catch (Exception exMega)
                    {
                        _logger.LogWarning(exMega, "No se pudo eliminar el archivo de MEGA para el documento {DocumentoId}", documentoId);
                        // Continuar con la eliminación de la BD
                    }
                }

                // ✅ Eliminar relaciones DocumentoTesorero
                _context.DocumentosTesoreros.RemoveRange(documento.DocumentoTesoreros);

                // ✅ Eliminar documento
                _context.Documentos.Remove(documento);

                await _context.SaveChangesAsync();

                return Ok("Documento eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el documento {DocumentoId}", documentoId);
                return StatusCode(500, new { message = "Error al eliminar el documento", error = ex.Message });
            }
        }

        [HttpGet("GetTesoreroActual")]
        [Authorize(Roles = "protesorero,tesorero,admin,auditor")]
        public async Task<ActionResult<TesoreroDisponibleDTO>> GetTesoreroActual()
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized("No se encontró el usuario.");
                }

                var usuario = await _context.Usuarios
                    .Where(u => u.UserName.ToLower() == userName.ToLower())
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado.");
                }

                if (usuario.IdMiembros == null || usuario.IdMiembros == 0)
                {
                    return BadRequest("El usuario no tiene un miembro asociado.");
                }

                var miembro = await _context.Miembros
                    .Where(m => m.IdMiembros == usuario.IdMiembros)
                    .FirstOrDefaultAsync();

                if (miembro == null)
                {
                    return NotFound("Miembro no encontrado.");
                }

                var tesorero = await _context.Tesoreros
                    .Where(t => t.IdMiembros == usuario.IdMiembros)
                    .FirstOrDefaultAsync();

                var dto = new TesoreroDisponibleDTO
                {
                    IdTesorero = usuario.IdMiembros ?? 0,
                    Nombre = $"{miembro.Nombre} {miembro.Apellido}"
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el tesorero actual.");
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }
    }
}
