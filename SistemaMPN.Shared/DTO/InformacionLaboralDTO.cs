using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class InformacionLaboralDTO
    {
        public int? id { get; set; }

        [MaxLength(40, ErrorMessage = "No puede superar los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string? situacion_laboral { get; set; }

        [MaxLength(40, ErrorMessage = "No puede superar los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string? rubro { get; set; }
        public bool tiene_informacion_laboral { get; set; } = false;
    }
}
