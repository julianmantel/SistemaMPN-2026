using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class PropuestaDeCambioDTO
    {
        public int IdPropuestaDeCambio { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        public int IdReceptor { get; set; }
        public int TurnoActual { get; set; } = new();
    }
}
