using CG.Web.MegaApiClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SistemaMPN.Modules.Archivos.Services
{
    public class MegaStorageService : IMegaStorageService, IDisposable
    {
        private readonly MegaApiClient _megaClient;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _operationTimeout = TimeSpan.FromSeconds(30);
        private readonly ILogger<MegaStorageService>? _logger;
        
        private string _email = string.Empty;
        private string _password = string.Empty;

        private INode? _carpetaMiembros;
        private INode? _carpetaTesoreria;
        
        private readonly ConcurrentDictionary<string, INode> _cacheCarpetasTipo = new();
        private readonly ConcurrentDictionary<string, (DateTime Timestamp, INode? Archivo)> _cacheArchivosTesoreria = new();
        
        private DateTime _ultimoRefreshNodos = DateTime.MinValue;
        private bool _inicializado = false;
        private readonly Timer? _timerRefresh;

        public MegaStorageService(IConfiguration configuration, ILogger<MegaStorageService>? logger = null)
        {
            _megaClient = new MegaApiClient();
            _logger = logger;

            _email = configuration["MegaStorage:Email"] ?? string.Empty;
            _password = configuration["MegaStorage:Password"] ?? string.Empty;

            if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_password))
            {
                throw new InvalidOperationException("Credenciales de MEGA configuradas de forma incorrecta");
            }

            _timerRefresh = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
            _timerRefresh.Elapsed += async (s, e) => await RefrescarCacheAsync();
            _timerRefresh.AutoReset = true;
            _timerRefresh.Start();
        }

        public async Task InitializeAsync()
        {
            if (_inicializado) return;
            
            await _semaphore.WaitAsync();
            try
            {
                if (_inicializado) return;
                
                _megaClient.Login(_email, _password);
                await InicializarCarpetasAsync();
                _inicializado = true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al inicializar MegaStorageService");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_inicializado || DateTime.UtcNow - _ultimoRefreshNodos > _cacheExpiry)
            {
                if (!_inicializado)
                {
                    await InitializeAsync();
                }
                else if (DateTime.UtcNow - _ultimoRefreshNodos > _cacheExpiry)
                {
                    await RefrescarCacheAsync();
                }
            }
        }

        private async Task RefrescarCacheAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                _ultimoRefreshNodos = DateTime.UtcNow;
                _cacheCarpetasTipo.Clear();
                _cacheArchivosTesoreria.Clear();
                _logger?.LogInformation("Cache de MEGA refrescado");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task InicializarCarpetasAsync()
        {
            var nodos = await _megaClient.GetNodesAsync();

            _carpetaMiembros = nodos.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name.Equals("Miembros", StringComparison.OrdinalIgnoreCase));
            _carpetaTesoreria = nodos.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name.Equals("Tesoreria", StringComparison.OrdinalIgnoreCase));

            if (_carpetaMiembros == null)
            {
                throw new InvalidOperationException("No se encontró la carpeta 'Miembros' en tu cuenta de MEGA. Por favor créala.");
            }

            if (_carpetaTesoreria == null)
            {
                throw new InvalidOperationException("No se encontró la carpeta 'Tesoreria' en tu cuenta de MEGA. Por favor créala.");
            }

            _ultimoRefreshNodos = DateTime.UtcNow;
            _logger?.LogInformation("Carpetas de MEGA inicializadas: Miembros={MiembrosId}, Tesoreria={TesoreriaId}", 
                _carpetaMiembros.Id, _carpetaTesoreria.Id);
        }

        private async Task<T> WithTimeout<T>(Task<T> task, string operationName)
        {
            using var cts = new CancellationTokenSource(_operationTimeout);
            try
            {
                return await task.WaitAsync(cts.Token);
            }
            catch (TimeoutException)
            {
                _logger?.LogWarning("Timeout en operación {OperationName} después de {Seconds}s", operationName, _operationTimeout.TotalSeconds);
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Operación {OperationName} cancelada después de {Seconds}s", operationName, _operationTimeout.TotalSeconds);
                throw new TimeoutException($"La operación {operationName} tomó demasiado tiempo");
            }
        }

        private async Task WithTimeout(Task task, string operationName)
        {
            using var cts = new CancellationTokenSource(_operationTimeout);
            try
            {
                await task.WaitAsync(cts.Token);
            }
            catch (TimeoutException)
            {
                _logger?.LogWarning("Timeout en operación {OperationName} después de {Seconds}s", operationName, _operationTimeout.TotalSeconds);
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Operación {OperationName} cancelada después de {Seconds}s", operationName, _operationTimeout.TotalSeconds);
                throw new TimeoutException($"La operación {operationName} tomó demasiado tiempo");
            }
        }

        #region Miembros
        public async Task<string> SubirFotoPerfilAsync(int miembroId, Stream archivoStream, string extension)
        {
            await EnsureInitializedAsync();

            if (!new[] { "jpg", "jpeg", "png" }.Contains(extension.ToLower()))
            {
                throw new InvalidOperationException("Formato de imagen no soportado. Formatos admitidos: jpg, jpeg y png");
            }

            if (archivoStream.Length > 10 * 1024 * 1024)
            {
                throw new ArgumentException("La imagen es demasiado grande. Tamaño máximo: 10MB");
            }

            var nombreArchivo = $"miembro_{miembroId}.{extension.ToLower()}";

            var archivosExistentes = await _megaClient.GetNodesAsync(_carpetaMiembros!);
            var archivoExistente = archivosExistentes.FirstOrDefault(n => n.Type == NodeType.File && n.Name.StartsWith($"miembro_{miembroId}."));

            if (archivoExistente != null)
            {
                await _megaClient.DeleteAsync(archivoExistente, false);
            }

            var nodo = await _megaClient.UploadAsync(archivoStream, nombreArchivo, _carpetaMiembros!);
            _logger?.LogDebug("Foto de perfil subida: miembro_{MiembroId}, archivo={Archivo}", miembroId, nombreArchivo);

            return nombreArchivo;
        }

        public async Task<(Stream Contenido, string Extension)?> ObtenerFotoPerfilAsync(int miembroId)
        {
            await EnsureInitializedAsync();

            var archivos = await _megaClient.GetNodesAsync(_carpetaMiembros!);
            var archivo = archivos.FirstOrDefault(n => n.Type == NodeType.File && n.Name.StartsWith($"miembro_{miembroId}."));

            if (archivo == null) return null;

            var stream = await _megaClient.DownloadAsync(archivo);
            var extension = Path.GetExtension(archivo.Name).TrimStart('.');

            return (stream, extension);
        }

        public async Task<bool> EliminarFotoPerfilAsync(int miembroId)
        {
            await EnsureInitializedAsync();

            var archivos = await _megaClient.GetNodesAsync(_carpetaMiembros!);
            var archivo = archivos.FirstOrDefault(n => n.Type == NodeType.File && n.Name.StartsWith($"miembro_{miembroId}."));

            if (archivo == null)
            {
                return false;
            }

            await _megaClient.DeleteAsync(archivo, false);
            return true;
        }
        #endregion

        #region Tesoreria
        public async Task<string> SubirDocumentoTesoreriaAsync(
            int documentoId,
            string tipoDocumento,
            bool esFirmado,
            Stream archivoStream)
        {
            await EnsureInitializedAsync();

            if (archivoStream.Length > 10 * 1024 * 1024)
            {
                throw new ArgumentException("El archivo PDF es demasiado grande. Tamaño máximo: 10MB");
            }

            var tipoNormalizado = tipoDocumento.Replace(" ", "").ToLower();
            var sufijo = esFirmado ? "_firmado" : "";
            var nombreArchivo = $"{tipoNormalizado}_{documentoId}{sufijo}.pdf";

            var nombreCarpetaTipo = CapitalizarPrimeraLetra(tipoDocumento.Replace(" ", ""));
            
            var carpetaTipo = await ObtenerCarpetaTipoAsync(nombreCarpetaTipo);

            var archivosEnCarpetaTask = _megaClient.GetNodesAsync(carpetaTipo);
            var uploadTask = _megaClient.UploadAsync(archivoStream, nombreArchivo, carpetaTipo);
            
            await Task.WhenAll(archivosEnCarpetaTask, uploadTask);
            
            var archivosEnCarpeta = await archivosEnCarpetaTask;
            var archivoExistente = archivosEnCarpeta.FirstOrDefault(n => n.Type == NodeType.File && n.Name.Equals(nombreArchivo, StringComparison.OrdinalIgnoreCase));
            
            if (archivoExistente != null)
            {
                await _megaClient.DeleteAsync(archivoExistente, false);
            }

            _logger?.LogInformation("Documento subido a MEGA: {NombreArchivo}", nombreArchivo);

            return nombreArchivo;
        }

        private async Task<INode> ObtenerCarpetaTipoAsync(string nombreCarpetaTipo)
        {
            var cacheKey = $"tesoreria_{nombreCarpetaTipo}";
            
            if (_cacheCarpetasTipo.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_cacheCarpetasTipo.TryGetValue(cacheKey, out var cached2))
                {
                    return cached2;
                }

                var nodos = await _megaClient.GetNodesAsync(_carpetaTesoreria!);
                var carpetaTipo = nodos.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name.Equals(nombreCarpetaTipo, StringComparison.OrdinalIgnoreCase));

                if (carpetaTipo == null)
                {
                    carpetaTipo = await _megaClient.CreateFolderAsync(nombreCarpetaTipo, _carpetaTesoreria!);
                    _logger?.LogInformation("Carpeta creada en MEGA: {NombreCarpeta}", nombreCarpetaTipo);
                }

                _cacheCarpetasTipo[cacheKey] = carpetaTipo;
                return carpetaTipo;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Stream?> ObtenerDocumentoTesoreriaAsync(int documentoId, string tipoDocumento, bool esFirmado)
        {
            await EnsureInitializedAsync();

            var tipoNormalizado = tipoDocumento.Replace(" ", "").ToLower();
            var sufijo = esFirmado ? "_firmado" : "";
            var nombreArchivo = $"{tipoNormalizado}_{documentoId}{sufijo}.pdf";

            var nombreCarpetaTipo = CapitalizarPrimeraLetra(tipoDocumento.Replace(" ", ""));
            
            if (!_cacheCarpetasTipo.TryGetValue($"tesoreria_{nombreCarpetaTipo}", out var carpetaTipo))
            {
                var nodos = await _megaClient.GetNodesAsync(_carpetaTesoreria!);
                carpetaTipo = nodos.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name.Equals(nombreCarpetaTipo, StringComparison.OrdinalIgnoreCase));
            }

            if (carpetaTipo == null) return null;

            var archivosEnCarpeta = await _megaClient.GetNodesAsync(carpetaTipo);
            var archivo = archivosEnCarpeta.FirstOrDefault(n => n.Type == NodeType.File && n.Name.Equals(nombreArchivo, StringComparison.OrdinalIgnoreCase));

            if (archivo == null) return null;

            return await _megaClient.DownloadAsync(archivo);
        }

        public async Task<bool> EliminarDocumentoTesoreriaAsync(int documentoId, string tipoDocumento, bool esFirmado)
        {
            await EnsureInitializedAsync();

            var tipoNormalizado = tipoDocumento.Replace(" ", "").ToLower();
            var sufijo = esFirmado ? "_firmado" : "";
            var nombreArchivo = $"{tipoNormalizado}_{documentoId}{sufijo}.pdf";

            var nombreCarpetaTipo = CapitalizarPrimeraLetra(tipoDocumento.Replace(" ", ""));
            
            if (!_cacheCarpetasTipo.TryGetValue($"tesoreria_{nombreCarpetaTipo}", out var carpetaTipo))
            {
                var nodos = await _megaClient.GetNodesAsync(_carpetaTesoreria!);
                carpetaTipo = nodos.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name.Equals(nombreCarpetaTipo, StringComparison.OrdinalIgnoreCase));
            }

            if (carpetaTipo == null)
            {
                return false;
            }

            var archivosEnCarpeta = await _megaClient.GetNodesAsync(carpetaTipo);
            var archivo = archivosEnCarpeta.FirstOrDefault(n => n.Type == NodeType.File && n.Name.Equals(nombreArchivo, StringComparison.OrdinalIgnoreCase));

            if (archivo == null)
            {
                return false;
            }

            await _megaClient.DeleteAsync(archivo, false);
            return true;
        }

        private static string CapitalizarPrimeraLetra(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            return char.ToUpper(texto[0]) + texto.Substring(1);
        }

        public void Dispose()
        {
            _timerRefresh?.Stop();
            _timerRefresh?.Dispose();
            _semaphore.Dispose();
            _megaClient?.Logout();
        }

        #endregion
    }
}