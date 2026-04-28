using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class EventoDTO
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
        public List<RolDTO> Roles { get; set; } = new();
        public bool Duracion_completa { get; set; }
    }
}
