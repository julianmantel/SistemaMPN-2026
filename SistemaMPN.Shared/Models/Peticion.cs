using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.Models
{
    public partial class Peticion
    {
        public int IdPeticiones { get; set; }

        public DateTime FechaSolicitud { get; set; }

        public DateTime? FechaRespuesta { get; set; }

        public string Estado { get; set; } = null!;

        public int? IdUsuario { get; set; }

        public int? IdLider { get; set; }

        public string? Mensaje { get; set; }

        public string? Tipo { get; set; }

        public virtual Usuario? IdUsuarioNavigation { get; set; }

        public virtual Miembro? IdLiderNavigation { get; set; }

        public virtual PeticionActualizacion? PeticionActualizacion { get; set; }

        public virtual PeticionCambio? PeticionCambio { get; set; }

        public virtual PeticionReunion? PeticionReunion { get; set; }

        public virtual PeticionAgregar? PeticionAgregar { get; set; }
    }
}
