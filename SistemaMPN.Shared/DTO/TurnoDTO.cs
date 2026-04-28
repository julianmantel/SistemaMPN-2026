using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaMPN.Shared.Attributes;

namespace SistemaMPN.Shared.DTO
{
    public class TurnoDTO : IValidatableObject
    {
        public int IdTurno { get; set; }

        [Required(ErrorMessage = "Asigne una fecha para el turno")]
        [DataType(DataType.DateTime)]
        [NotPastDate]
        public DateTime? Fecha { get; set; }

        [Required(ErrorMessage = "Asigne la hora de inicio del turno")]
        [DataType(DataType.Time)]
        public TimeSpan? HoraInicio { get; set; }
        public string Color { get; set; } = "#2196F3";
        public int IdTesorero { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Fecha.HasValue && HoraInicio.HasValue)
            {
                if (Fecha.Value.Date == DateTime.Today &&
                    HoraInicio.Value < DateTime.Now.TimeOfDay)
                {
                    yield return new ValidationResult(
                        "La hora de inicio no puede ser una hora pasada para la fecha actual.",
                        new[] { nameof(HoraInicio) }
                    );
                }
            }
        }
    }
}
