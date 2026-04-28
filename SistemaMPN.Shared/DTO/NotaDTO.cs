using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class NotaDTO
    {
        public int IdNota { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio")]
        [MaxLength(200, ErrorMessage = "El comentario no puede exceder los 200 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El comentario debe contener letras y no puede ser solo números o símbolos")]
        public string? Comentarios { get; set; }
        public int IdReunion { get; set; }
    }
}
