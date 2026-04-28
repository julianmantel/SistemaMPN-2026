using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class InformacionPersonalDTO
    {
        public int? id { get; set; }

        [MaxLength(30, ErrorMessage = "No puede superar los 30 caracteres")]
        [RegularExpression(@"^(\+\d{1,2})?\ ?\d{1,2}?\ ?\d{3,4}\ ?\d{6,7}$", ErrorMessage = "Número de teléfono inválido")]
        public string telefono_alternativo { get; set; } = string.Empty;

        [MaxLength(30, ErrorMessage = "No puede superar los 30 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string? estado_civil { get; set; }

        [MaxLength(70, ErrorMessage = "El nombre de la pareja no puede superar los 70 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
        public string? pareja { get; set; }
        public List<HijoDTO> hijos { get; set; } = new List<HijoDTO>();
        public bool tiene_informacion_personal { get; set; } = false;
    }
}
