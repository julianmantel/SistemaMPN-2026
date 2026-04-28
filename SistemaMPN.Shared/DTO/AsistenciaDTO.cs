using System;

namespace SistemaMPN.Shared.DTO;

public class AsistenciaDTO
{
    public int IdAsistencia { get; set; }
    public int? MiembroId { get; set; }
    public string? NombreMiembro { get; set; }
    public string? ApellidoMiembro { get; set; }
    public string? NombreVisitante { get; set; }
    public string? ApellidoVisitante { get; set; }
    public string? EmailVisitante { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string Origen { get; set; } = string.Empty;
    public int EventoId { get; set; }
    public string? TituloEvento { get; set; }
    public bool EsMiembroRegistrado => MiembroId.HasValue;
}
