using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class ReciboGeneralDTO
    {
        public DateOnly Fecha;

        [Required(ErrorMessage = "El número del recibo es obligatorio")]
        public int Numero { get; set; }
        public decimal MontoNum {  get; set; }
        public string MontoLetras { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "No puede superar los 200 caracteres")]
        public string Concepto { get; set; } = string.Empty;
        public bool Automatico { get; set; }
    }
}
