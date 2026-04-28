using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Tesorero
{
    public int IdMiembros { get; set; }

    public bool IsPro { get; set; }

    public virtual Miembro IdMiembrosNavigation { get; set; } = null!;

    public virtual ICollection<DocumentoTesorero> DocumentoTesoreros { get; set; } = new List<DocumentoTesorero>();

    public virtual ICollection<PropuestaCambioTurno> PropuestaCambioTurnos { get; set; } = new List<PropuestaCambioTurno>();

    public virtual ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
