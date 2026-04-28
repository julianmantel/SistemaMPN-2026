using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Consultor
{
    public int IdMiembros { get; set; }

    public virtual Miembro IdMiembrosNavigation { get; set; } = null!;

    public virtual ICollection<Reunion> Reuniones { get; set; } = new List<Reunion>();
}
