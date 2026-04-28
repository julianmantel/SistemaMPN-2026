using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class DetalleCajaDTO
    {
        public string Reunion { get; set; } = "";

        [Required(ErrorMessage = "El codigo alfanumerico es obligatorio")]
        [MaxLength(50, ErrorMessage = "El codigo alfanumerico no puede superar los 50 caracteres")]
        public string NumeroALF { get; set; } = "";
        public List<DenominacionDTO> Denominaciones { get; set; } = new();
        public decimal TotalUSD { get; set; }

        [MaxLength(300, ErrorMessage = "Las observaciones no pueden superar los 400 caracteres")]
        public string Observaciones { get; set; } = "";
    }
}
