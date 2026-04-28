using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SistemaMPN.Shared.Models
{
    public partial class Turno
    {
        public int IdTurnos { get; set; }

        public string? Color { get; set; }

        public DateTime? Fecha { get; set; }

        public TimeSpan? HoraInicio { get; set; }

        public int? IdTesorero { get; set; }

        public virtual Tesorero? IdTesoreroNavigation { get; set; }

        public virtual ICollection<PropuestaCambioTurno> PropuestaCambioTurnos { get; set; } = new List<PropuestaCambioTurno>();
    }
}
