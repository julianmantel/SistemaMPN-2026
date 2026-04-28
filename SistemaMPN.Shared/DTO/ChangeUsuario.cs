using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class ChangeUsuario
    {
        public string old_user_name { get; set; } = string.Empty;
        public string new_user_name { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public List<RolDTO> roles { get; set; } = new List<RolDTO>();
    }
}
