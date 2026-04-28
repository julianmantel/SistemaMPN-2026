using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace SistemaMPN.Shared.Models;

public partial class Localizacion
{
    public int IdLocalizaciones { get; set; }
    public string? Tipo { get; set; }

    public string? Direccion { get; set; }

    public NpgsqlPoint Ubicacion { get; set; }

    public virtual ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
}
