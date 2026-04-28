using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.Models
{
    public partial class PropuestaCambioTurno
    {
        public int IdPropuestaCambioTurno { get; set; }

        public string Estado { get; set; } = null!;

        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRespuesta { get; set; }

        public int? IdReceptor { get; set; }

        public int? IdTurno { get; set; }

        public virtual Tesorero? IdReceptorNavigation { get; set; }

        public virtual Turno? IdTurnoNavigation { get; set; }
    }
}
