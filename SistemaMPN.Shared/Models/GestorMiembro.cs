using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class GestorMiembro
{
    public int IdMiembros { get; set; }

    public virtual Miembro IdMiembrosNavigation { get; set; } = null!;
}
