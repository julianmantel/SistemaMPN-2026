using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class InfoMiembroGrupoDTO
    {
        public int id_miembro { get; set; }
        public string nombre_completo { get; set; } = string.Empty;
        public string dni { get; set; } = string.Empty;

        public string ocupacion { get; set; } = string.Empty;
        public DateOnly? fecha_desde { get; set; } = null;
        public bool tiene_usuario { get; set; }
        public bool puede_ser_lider { get; set; }
    }
}
