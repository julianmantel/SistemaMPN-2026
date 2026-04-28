using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class PerteneceGrupo
{
    public int IdGrupos { get; set; }

    public int IdMiembros { get; set; }

    public DateOnly? FechaDesde { get; set; }

    public string? Ocupacion { get; set; }

    public virtual Grupo IdGruposNavigation { get; set; } = null!;

    public virtual Miembro IdMiembrosNavigation { get; set; } = null!;
}
