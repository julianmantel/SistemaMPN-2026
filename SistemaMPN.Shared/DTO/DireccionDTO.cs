using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class DireccionDTO
    {
        public int? id { get; set; }

        [MinLength(3, ErrorMessage = "El nombre de la calle debe tener al menos 3 caracteres")]
        [MaxLength(40, ErrorMessage = "El nombre de la calle no puede exceder los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El nombre de la calle debe contener letras y no puede ser solo números o símbolos")]
        public string? calle { get; set; }
        public int? altura { get; set; }

        [MaxLength(50, ErrorMessage = "El nombre del barrio no puede exceder los 50 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El nombre del barrio debe contener letras y no puede ser solo números o símbolos")]
        public string barrio { get; set; } = string.Empty;
        public bool tiene_direccion { get; set; } = false;
    }
}
