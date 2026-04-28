using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.Models
{
    public partial class Notificacion
    {
        public int IdNotificaciones { get; set; }

        public string? Mensaje { get; set; }

        public DateTime? Fecha { get; set; }

        public string? Tipo { get; set; }

        public virtual ICollection<NotificacionUsuario> NotificacionUsuarios { get; set; } = new List<NotificacionUsuario>();
    }
}
