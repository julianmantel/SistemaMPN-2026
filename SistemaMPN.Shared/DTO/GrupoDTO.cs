using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class GrupoDTO
    {
        public int id_grupo { get; set; }
        public int? id_lider { get; set; }
        public string lider_nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del grupo es obligatorio")]
        [MinLength(3, ErrorMessage = "El nombre del grupo debe tener al menos 3 caracteres")]
        [MaxLength(50, ErrorMessage = "El nombre del grupo no puede superar los 50 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El nombre del grupo debe contener letras y no puede ser solo números o símbolos")]
        public string nombre { get; set; } = string.Empty;
        public int? cantidad_miembros { get; set; }
        public int? max_cant_miembros { get; set; }
        public int? id_localizaciones { get; set; }
    }
}
