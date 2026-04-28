using System;

namespace SistemaMPN.Shared.DTO;

public class QrEventoDTO
{
    public int EventoId { get; set; }
    public string TituloEvento { get; set; } = string.Empty;
    public string CodigoVerificacion { get; set; } = string.Empty;
    public string UrlCheckIn { get; set; } = string.Empty;
}
