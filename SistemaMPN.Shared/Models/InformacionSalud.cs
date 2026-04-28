using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class InformacionSalud
{
    public int IdInformacionSalud { get; set; }

    public string? GrupoSanguineo { get; set; }

    public string? Observaciones { get; set; }

    public virtual Miembro? Miembro { get; set; }

    public virtual ICollection<TelefonoEmergencia> TelefonosEmergencia { get; set; } = new List<TelefonoEmergencia>();

    public virtual ICollection<Alergia> IdAlergia { get; set; } = new List<Alergia>();

    public virtual ICollection<CondicionMedica> IdCondicionMedicas { get; set; } = new List<CondicionMedica>();

    public virtual ICollection<Medicamento> IdMedicamentos { get; set; } = new List<Medicamento>();
}
