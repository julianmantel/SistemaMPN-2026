using SistemaMPN.Shared.Models;
using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class SeminariosCursado
{
    public int IdSeminario { get; set; }

    public int IdInformacionEclesiastica { get; set; }

    public int? AnioCursado { get; set; }

    public string? Estado { get; set; }

    public virtual InformacionEclesiastica IdInformacionEclesiasticaNavigation { get; set; } = null!;

    public virtual Seminario IdSeminariosNavigation { get; set; } = null!;
}
