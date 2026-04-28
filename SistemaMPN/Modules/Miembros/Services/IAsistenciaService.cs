using SistemaMPN.Shared.DTO;

namespace SistemaMPN.Modules.Miembros.Services;

public interface IAsistenciaService
{
    Task<QrEventoDTO> GenerarCodigoEventoAsync(int eventoId);
    Task<string> RegistrarAsistenciaAsync(RegistroAsistenciaDTO registro);
    Task<List<AsistenciaDTO>> GetAsistentesEventoAsync(int eventoId);
    Task<List<AsistenciaDTO>> GetHistorialMiembroAsync(int miembroId, DateTime? desde = null, DateTime? hasta = null);
    Task<int> GetCantidadAsistentesAsync(int eventoId);
    Task<bool> YaRegistroAsync(int? miembroId, string? email, int eventoId);
}
