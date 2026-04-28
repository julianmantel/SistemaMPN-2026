using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class Grupo
{
    public int IdGrupos { get; set; }

    public string? Nombre { get; set; }

    public int? CantidadMiembros { get; set; }

    public int? MaxCantMiembros { get; set; } = 12;

    public int? IdLocalizaciones { get; set; }

    public virtual Localizacion? IdLocalizacionesNavigation { get; set; }

    public virtual ICollection<PerteneceGrupo> PerteneceGrupos { get; set; } = new List<PerteneceGrupo>();

    public virtual ICollection<PeticionCambio> PeticionesCambio { get; set; } = new List<PeticionCambio>();
}
