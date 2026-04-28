using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class ForgotPasswordDTO
    {
        public string Correo { get; set; } = string.Empty;
    }
}
