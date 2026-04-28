using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Hijo
{
    public int IdHijos { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public virtual ICollection<DatoPersonal> IdDatosPersonales { get; set; } = new List<DatoPersonal>();
}
