using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class BackupDTO
    {
        public string Nombre { get; set; } = "";
        public string Tamano { get; set; } = "";
        public DateTime FechaCreacion { get; set; }
    }
}
