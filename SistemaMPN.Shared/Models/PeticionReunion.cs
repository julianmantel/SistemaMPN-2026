using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.Models
{
    public partial class PeticionReunion
    {
        public int IdPeticiones { get; set; }

        public string? Motivo { get; set; }

        public DateOnly? FechaPreferida { get; set; }

        public string? Correo { get; set; }

        public int? IdMiembros { get; set; }

        public virtual Miembro? IdMiembrosNavigation { get; set; }

        public virtual Peticion IdPeticionesNavigation { get; set; } = null!;
    }
}
