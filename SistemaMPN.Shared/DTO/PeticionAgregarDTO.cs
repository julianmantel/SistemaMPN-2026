using System.ComponentModel.DataAnnotations;

namespace SistemaMPN.Shared.DTO
{
    public class PeticionAgregarDTO
    {
        public int IdPeticiones { get; set; }
        public int IdPeticionAgregar { get; set; }

        [MaxLength(10, ErrorMessage = "No puede superar los 10 caracteres")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El DNI solo puede contener dígitos")]
        public string? Dni { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MinLength(2, ErrorMessage = "El apellido debe tener al menos 2 caracteres")]
        [MaxLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El apellido solo puede contener letras")]
        public string Apellido { get; set; } = null!;
        public DateOnly? FechaNacimiento { get; set; }
        public string? Nacionalidad { get; set; }

        [MaxLength(50, ErrorMessage = "El nombre del lugar no puede superar los 50 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El nombre del lugar debe contener letras y no puede ser solo números o símbolos")]
        public string? LugarNacimiento { get; set; }

        [Required(ErrorMessage = "El numero de telefono es obligatorio")]
        [MaxLength(30, ErrorMessage = "El numero de telefono no puede superar los 30 caracteres")]
        [RegularExpression(@"^(\+\d{1,2})?\ ?\d{1,2}?\ ?\d{3,4}\ ?\d{6,7}$", ErrorMessage = "Número de teléfono inválido")]
        public string? Telefono { get; set; }
        public char Sexo { get; set; } = 'M';
        public PeticionDTO Peticion { get; set; } = new() { Tipo = "Agregar" };
    }
}
