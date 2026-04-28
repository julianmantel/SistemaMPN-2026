using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class InformacionEclesiastica
{
    public int IdInformacionEclesiastica { get; set; }

    public int? IdBautismos { get; set; }

    public string? Convocante { get; set; }

    public int? FechaAsiste { get; set; }

    public virtual Bautismo? IdBautismoNavigation { get; set; }

    public virtual Miembro? Miembro { get; set; }

    public virtual ICollection<SeminariosCursado> SeminariosCursados { get; set; } = new List<SeminariosCursado>();
}
