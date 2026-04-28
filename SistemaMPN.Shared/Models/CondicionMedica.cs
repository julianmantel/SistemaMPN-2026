using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class CondicionMedica
{
    public int IdCondicionMedica { get; set; }

    public string? Condicion { get; set; }

    public virtual ICollection<InformacionSalud> IdInformacionSalud { get; set; } = new List<InformacionSalud>();
}
