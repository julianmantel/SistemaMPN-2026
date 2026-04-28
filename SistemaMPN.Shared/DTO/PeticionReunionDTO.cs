using SistemaMPN.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class PeticionReunionDTO
    {
        [MaxLength(300, ErrorMessage = "No puede superar los 300 caracteres")]
        public string? Motivo { get; set; } = String.Empty;

        public DateOnly? FechaPreferida { get; set; }

        [Required(ErrorMessage = "Ingrese el correo del participante de la reunion")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Correo { get; set; } = String.Empty;
        public int? IdMiembros { get; set; }
        public PeticionDTO Peticion { get; set; } = new PeticionDTO { Tipo = "Reunion" };
        public int? IdConsultor { get; set; }
    }
}
