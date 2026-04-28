using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Bautismo
{
    public int IdBautismos { get; set; }

    public bool? Realizo { get; set; }

    public int? Fecha { get; set; }

    public string? Lugar { get; set; }

    public string? Pastor { get; set; }

    public virtual ICollection<InformacionEclesiastica> InformacionesEclesiasticas { get; set; } = new List<InformacionEclesiastica>();
}
