using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;

namespace SistemaMPN.Shared.DTO
{
    public class InfoMinimaMiembroDTO
    {
        public int? id { get; set; }
        public string nombre_completo { get; set; } = string.Empty;
        public string dni { get; set; } = string.Empty;
        public string telefono { get; set; } = string.Empty;
        public string telefono_fijo { get; set; } = string.Empty;
        public DateTime? fecha_creacion { get; set; }
    }
}
