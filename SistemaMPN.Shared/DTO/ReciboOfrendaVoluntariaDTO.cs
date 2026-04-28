using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class ReciboOfrendaVoluntariaDTO
    {
        public DateOnly Fecha;

        [Required(ErrorMessage = "El número del recibo es obligatorio")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "El nombre del grupo es obligatorio")]
        [MinLength(3, ErrorMessage = "El nombre del grupo debe tener al menos 3 caracteres")]
        [MaxLength(50, ErrorMessage = "El nombre del grupo no puede superar los 50 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El nombre del grupo debe contener letras y no puede ser solo números o símbolos")]
        public string Grupo { get; set; } = string.Empty;
        public decimal Pesos { get; set; }
        public decimal Dolares { get; set; }
        public string Otros { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "No puede superar los 200 caracteres")]
        public string Para { get; set; } = string.Empty;
    }
}
