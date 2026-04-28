using NpgsqlTypes;
using SistemaMPN.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class LocalizacionDTO
    {
        public int IdLocalizacion { get; set; }

        [Required(ErrorMessage = "Se requiere un tipo de localización")]
        public string? Tipo { get; set; } = string.Empty;

        [MinLength(3, ErrorMessage = "La dirección debe tener al menos 3 caracteres")]
        [MaxLength(40, ErrorMessage = "La dirección no puede exceder los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "La dirección debe contener letras y no puede ser solo números o símbolos")]
        [Required(ErrorMessage = "Asigne una direccion valida")]
        public string? Direccion { get; set; } = string.Empty;

        public NpgsqlPoint Ubicacion { get; set; }
        
        public GrupoDTO Grupo { get; set; } = new();
    }
}
