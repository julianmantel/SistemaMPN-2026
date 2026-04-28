using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class UsuarioDTO
    {
        [Required(ErrorMessage ="El nombre de usuario es obligatorio")]
        public string user_name {  get; set; } = string.Empty;
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string password { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public List<RolDTO> roles { get; set; } = new List<RolDTO>();
        public string miembro_dni { get; set; } = string.Empty;
        public string recaptcha_token { get; set; } = string.Empty;
    }
}
