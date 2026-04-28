using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class TelefonoEmergenciaDTO
    {
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(\+\d{1,2})?\ ?\d{1,2}?\ ?\d{3,4}\ ?\d{6,7}$", ErrorMessage = "Número de teléfono inválido")]
        [Required(ErrorMessage = "Ingrese el numero del telefono de emergencia")]
        public string? nro_telefono { get; set; }

        [Required(ErrorMessage = "Ingrese el propietario del numero")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        [MaxLength(70, ErrorMessage = "El nombre no puede superar los 70 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
        public string? propietario_telefono { get; set; }
    }
}
