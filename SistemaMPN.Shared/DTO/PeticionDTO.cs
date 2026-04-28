using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class PeticionDTO
    {
        public int IdPeticiones { get; set; }
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
        public DateTime? FechaRespuesta { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? Mensaje { get; set; } = String.Empty;
        public string? Tipo { get; set; } = String.Empty;
        public int? IdUsuario { get; set; }
        public int? IdLider { get; set; }
    }
}
