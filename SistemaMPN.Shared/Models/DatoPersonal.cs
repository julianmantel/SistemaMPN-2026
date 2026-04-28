using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class DatoPersonal
{
    public int IdDatosPersonales { get; set; }

    public string? TelefonoAlternativo { get; set; }

    public string EstadoCivil { get; set; } = null!;

    public string? Pareja { get; set; }

    public virtual Miembro? Miembro { get; set; }

    public virtual ICollection<Hijo> IdHijos { get; set; } = new List<Hijo>();
}
