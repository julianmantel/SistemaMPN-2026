using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.Models
{
    public class Reunion
    {
        public int IdReunion { get; set; }
        public DateTime Fecha { get; set; }
        public string? Motivo { get; set; }
        public string? Correo { get; set; }
        public string? Estado { get; set; }

        // Claves foráneas que te faltan
        public int IdMiembro { get; set; }
        public int IdConsultor { get; set; }

        // ID de petición de reunión (cuando viene de una solicitud de líder)
        public int? IdPeticion { get; set; }

        // Propiedades de navegación (opcional pero recomendado)
        public Miembro? Miembro { get; set; }
        public Consultor? Consultor { get; set; }

        public ICollection<Nota>? Notas { get; set; } = new List<Nota>();
    }
}
