using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Client.Modules.Miembros.ViewModels
{
    public class MiembroViewModel
    {
        public InfoMinimaMiembroDTO Miembro { get; set; }
        public bool MostrarDetalle { get; set; } = false;
    }
}
