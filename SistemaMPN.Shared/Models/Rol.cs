using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Rol
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Evento> IdEventos { get; set; } = new List<Evento>();

    public virtual ICollection<Usuario> IdUsuarios { get; set; } = new List<Usuario>();
}
