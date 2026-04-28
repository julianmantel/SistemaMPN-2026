using System.ComponentModel.DataAnnotations;

namespace SistemaMPN.Shared.DTO
{
    public class ResetPasswordDTO
    {
        public string ResetToken { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Ingresar nueva contraseña")]
        public string? NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        [Display(Name = "Confirmar nueva contraseña")]
        public string? ConfirmNewPassword { get; set; }
    }
}
