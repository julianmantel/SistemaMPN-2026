using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class RegistroPersonaDTO
    {
        [MaxLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
        public string NombreApellido { get; set; } = "";

        [MaxLength(100, ErrorMessage = "La observacion no puede superar los 100 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "La observacion debe contener letras y no puede ser solo números o símbolos")]
        public string Observacion { get; set; } = "";
    }
}
