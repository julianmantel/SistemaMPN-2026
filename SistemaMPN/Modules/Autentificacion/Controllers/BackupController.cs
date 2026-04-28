using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaMPN.Modules.Autentificacion.Services;

namespace SistemaMPN.Modules.Autentificacion.Controllers
{
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _backupService;
        private readonly IWebHostEnvironment _env;

        public BackupController(IBackupService backupService, IWebHostEnvironment env)
        {
            _backupService = backupService;
            _env = env;
        }

        [HttpGet("listar")]
        public IActionResult Listar()
        {
            var backups = _backupService.GetBackups().Select(f => new
            {
                Nombre = f.Name,
                Tamano = $"{(f.Length / 1024.0 / 1024.0):F2} MB",
                FechaCreacion = f.LastWriteTime
            });

            return Ok(backups);
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear(CancellationToken cancellationToken)
        {
            try
            {
                var fileName = await _backupService.CreateBackupAsync(cancellationToken);
                return Ok(new { Message = "Backup creado", Archivo = fileName });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, "El backup fue cancelado o excedió el tiempo límite.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("restaurar")]
        public async Task<IActionResult> Restaurar([FromQuery] string archivo, CancellationToken cancellationToken)
        {
            try
            {
                await _backupService.RestoreBackupAsync(archivo, cancellationToken);
                return Ok(new { Message = "Restauración completada" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(408, "La restauración fue cancelada o excedió el tiempo límite.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("descargar/{archivo}")]
        public IActionResult Descargar(string archivo)
        {
            var path = _backupService.GetBackupPath(archivo);
            if (!System.IO.File.Exists(path)) return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;
            return File(memory, "application/sql", archivo);
        }

    }
}
