using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Trayectoria
{
    public int IdTrayectoria { get; set; }

    public string EstudiosPrimario { get; set; } = null!;

    public string EstudiosSecundario { get; set; } = null!;

    public string EstudiosTerciario { get; set; } = null!;

    public string EstudiosUniversitario { get; set; } = null!;

    public string? Carrera { get; set; }

    public string? SituacionLaboral { get; set; }

    public string? Rubro { get; set; }

    public string? CursosRealizados { get; set; }

    public virtual Miembro? Miembro { get; set; }
}
