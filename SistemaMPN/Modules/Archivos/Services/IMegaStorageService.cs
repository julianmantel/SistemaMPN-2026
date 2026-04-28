using System.IO;

namespace SistemaMPN.Modules.Archivos.Services
{
    public interface IMegaStorageService
    {
        Task InitializeAsync();
        
        Task<string> SubirFotoPerfilAsync(int miembroId, Stream archivoStream, string extension);
        Task<(Stream Contenido, string Extension)?> ObtenerFotoPerfilAsync(int miembroId);
        Task<bool> EliminarFotoPerfilAsync(int miembroId);
        
        Task<string> SubirDocumentoTesoreriaAsync(int documentoId, string tipoDocumento, bool esFirmado, Stream archivoStream);
        Task<Stream?> ObtenerDocumentoTesoreriaAsync(int documentoId, string tipoDocumento, bool esFirmado);
        Task<bool> EliminarDocumentoTesoreriaAsync(int documentoId, string tipoDocumento, bool esFirmado);
    }
}