using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaMPN.Shared.Models;

public partial class Asistencia
{
    [Key]
    public int IdAsistencia { get; set; }

    public int? MiembroId { get; set; }

    public int EventoId { get; set; }

    public string? NombreVisitante { get; set; }

    public string? ApellidoVisitante { get; set; }

    public string? EmailVisitante { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string CodigoVerificacion { get; set; } = string.Empty;

    public string Origen { get; set; } = "QR";

    public virtual Evento? Evento { get; set; }

    public virtual Miembro? Miembro { get; set; }
}
