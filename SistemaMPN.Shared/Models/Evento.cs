using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor.Utilities;

namespace SistemaMPN.Shared.Models
{
    public partial class Evento
    {
        public int IdEvento { get; set; }

        public string? Titulo { get; set; }

        public string? Lugar { get; set; }

        public string? Calle { get; set; }

        public int? Altura { get; set; }

        public string Color { get; set; } = "#2196F3";

        public DateTime? Fecha { get; set; }

        public TimeSpan? HoraInicio { get; set; }

        public TimeSpan? HoraFin { get; set; }

        public virtual ICollection<Rol> IdRols { get; set; } = new List<Rol>();

        public bool Duracion_completa { get; set; } = false;
    }
}
