using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Seminario
{
    public int IdSeminario { get; set; }

    public string? Nombre { get; set; }

    public int? AnioComienzo { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<SeminariosCursado> SeminariosCursados { get; set; } = new List<SeminariosCursado>();
}
