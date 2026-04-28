using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Alergia
{
    public int IdAlergia { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<InformacionSalud> IdInformacionSalud { get; set; } = new List<InformacionSalud>();
}
