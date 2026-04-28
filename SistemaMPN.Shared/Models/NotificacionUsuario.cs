using System;
using System.Collections.Generic;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Shared.Models
{
    public partial class NotificacionUsuario
    {
        public int IdNotificacion { get; set; }

        public int IdUsuario { get; set; }

        public bool? Leida { get; set; }

        public virtual Notificacion IdNotificacionNavigation { get; set; } = null!;

        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    }
}
