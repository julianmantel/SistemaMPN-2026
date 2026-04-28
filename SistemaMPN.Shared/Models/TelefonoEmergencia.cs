using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

public partial class TelefonoEmergencia
{
    public int IdTelefonosEmergencia { get; set; }

    public int? IdInformacionSalud { get; set; }

    public string? Telefono { get; set; }

    public string? Propietario { get; set; }

    public virtual InformacionSalud IdInformacionSaludNavigation { get; set; } = null!;
}
