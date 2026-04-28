using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SistemaMPN.Shared.DTO
{
    public class DocumentoDTO
    {
        public int id_documento { get; set; }
        public int nro_documento { get; set; } 
        public DateOnly fecha { get; set; }
        public string? tipo { get; set; } // Agrega el "?"
        public bool firmado { get; set; }

        // Estos campos fallan porque el validador los ve como obligatorios
        public int id_tesorero { get; set; }
        public string? nombre_tesorero { get; set; } // Agrega el "?" para que no sea requerido
        public string? cargo_tesorero { get; set; }  // Agrega el "?"

        public int? id_supervisor { get; set; }
        public string? nombre_supervisor { get; set; } // Agrega el "?"
        public string? cargo_supervisor { get; set; }  // Agrega el "?"
    }
}
