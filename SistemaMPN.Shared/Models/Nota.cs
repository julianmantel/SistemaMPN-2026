using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.Models
{
    public class Nota
    {
        public int IdNota { get; set; }
        public string? Comentarios { get; set; }
        public int IdReunion { get; set; }

        // Navegación
        public Reunion? Reunion { get; set; } 
    }
}
