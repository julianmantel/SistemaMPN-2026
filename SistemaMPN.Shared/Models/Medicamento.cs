using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Medicamento
{
    public int IdMedicamento { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<InformacionSalud> IdInformacionSalud { get; set; } = new List<InformacionSalud>();
}
