using System;
using System.ComponentModel.DataAnnotations;

namespace SistemaMPN.Shared.DTO;

public class RegistroAsistenciaDTO
{
    [Required(ErrorMessage = "El código del evento es requerido")]
    public string CodigoEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
    [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [MinLength(2, ErrorMessage = "El apellido debe tener al menos 2 caracteres")]
    [MaxLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El apellido solo puede contener letras")]
    public string Apellido { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "No puede superar los 20 caracteres")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "El DNI solo puede contener dígitos")]
    public string? Dni { get; set; }

    [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
    public string? Email { get; set; }
}
