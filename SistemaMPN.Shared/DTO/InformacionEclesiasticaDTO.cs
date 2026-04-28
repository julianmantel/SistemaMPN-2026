using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class InformacionEclesiasticaDTO : IValidatableObject
    {
        public int? id { get; set; }

        [MaxLength(70, ErrorMessage = "El nombre del convocante no puede superar los 70 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string? convocante { get; set; }
        public int? fecha_desde_la_que_asiste { get; set; } = null;
        public int? fecha_de_bautismo { get; set; } = null;
        public bool? realizo_bautismo { get; set; } = null;

        [MaxLength(70, ErrorMessage = "El nombre del pastor no puede superar los 70 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string? pastor { get; set; }

        [MaxLength(70, ErrorMessage = "El nombre del lugar del bautismo no puede superar los 50 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string? lugar_bautismo { get; set; }
        public List<SeminarioDTO> seminarios { get; set; } = new List<SeminarioDTO>();
        public bool tiene_informacion_eclesiastica { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(realizo_bautismo != null && realizo_bautismo.Value)
            {
                if(fecha_de_bautismo == null)
                {
                    yield return new ValidationResult("El año del bautismo es obligatorio si se realizó el bautismo", new[] { nameof(fecha_de_bautismo) });
                }
            }
        }
    }
}
