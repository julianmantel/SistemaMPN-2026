using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.Models
{
    public class PeticionCambiarPassword
    {
        public int IdPeticionCambiarPassword { get; set; }
        public string ResetToken { get; set; } = string.Empty;
        public DateTime? ResetTokenExpire { get; set; }

        public int? IdUsuarios { get; set; }
        public virtual Usuario? IdUsuariosNavigation { get; set; }
    }
}
