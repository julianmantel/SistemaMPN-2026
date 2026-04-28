using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class InfoBasicaMiembroDTO : IValidatableObject
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
        public string nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MinLength(2, ErrorMessage = "El apellido debe tener al menos 2 caracteres")]
        [MaxLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El apellido solo puede contener letras")]
        public string apellido { get; set; } = null!;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [MaxLength(10, ErrorMessage = "No puede superar los 10 caracteres")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El DNI solo puede contener dígitos")]
        public string dni { get; set; } = null!;

        [Required(ErrorMessage = "La nacionalidad es obligatoria")]
        public string? nacionalidad { get; set; }

        [MaxLength(50, ErrorMessage = "El nombre del lugar no puede superar los 50 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El nombre del lugar debe contener letras y no puede ser solo números o símbolos")]
        public string? lugarNacimiento { get; set; }

        public char sexo { get; set; } = 'X';

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha de nacimiento debe ser una fecha válida")]
        public DateTime? fecha_nacimiento { get; set; } = null;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(30, ErrorMessage = "No puede superar los 30 caracteres")]
        [RegularExpression(@"^(\+\d{1,2})?\ ?\d{1,2}?\ ?\d{3,4}\ ?\d{6,7}$", ErrorMessage = "Número de teléfono inválido")]
        public string telefono { get; set; } = null!;

        [MaxLength(30, ErrorMessage = "No puede superar los 30 caracteres")]
        [RegularExpression(@"^(\+\d{1,2})?\ ?\d{1,2}?\ ?\d{3,4}\ ?\d{6,7}$", ErrorMessage = "Número de teléfono inválido")]
        public string? telefono_fijo { get; set; }

        public DateTime? fecha_creacion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(fecha_nacimiento != null && fecha_nacimiento.Value > DateTime.Now.AddYears(-16))
            {
                yield return new ValidationResult("El miembro debe tener al menos 16 años de edad", new[] { nameof(fecha_nacimiento) });
            }
        }
    }
}
