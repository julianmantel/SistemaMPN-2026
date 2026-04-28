using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Lider
{
    public int IdMiembros { get; set; }

    public string? Tipo { get; set; }

    public virtual Miembro IdMiembrosNavigation { get; set; } = null!;
}
