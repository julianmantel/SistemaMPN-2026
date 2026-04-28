using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Direccion
{
    public int IdDireccion { get; set; }

    public string? Calle { get; set; }

    public int? Altura { get; set; }

    public string? Barrio { get; set; }

    public virtual Miembro? Miembro { get; set; }
}
