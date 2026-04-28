using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class SeminarioDTO
    {
        public int id { get; set; } 

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El nombre debe contener letras y no puede ser solo números o símbolos")]
        public string? nombre { get; set; }

        [Range(1900, 2100, ErrorMessage = "Debe ingresar un año válido entre 1900 y 2100.")]
        public int? anio_comienzo { get; set; } = null;  
        public bool activo { get; set; } = false;
        public int? anio_cursado { get; set; }

        [MaxLength(25, ErrorMessage = "El estado no puede superar los 25 caracteres")]
        public string estado { get; set; } = string.Empty;
    }
}
