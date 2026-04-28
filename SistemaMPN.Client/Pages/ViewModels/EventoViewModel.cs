using System.ComponentModel.DataAnnotations;
using Heron.MudCalendar;
using MudBlazor.Utilities;
using SistemaMPN.Shared.Attributes;
using SistemaMPN.Shared.DTO;

namespace SistemaMPN.Client.Pages.ViewModels
{
    public class EventoViewModel : CalendarItem, IValidatableObject
    {
        public int IdEvento { get; set; }

        [MinLength(3, ErrorMessage = "El titulo del evento debe tener al menos 3 caracteres")]
        [MaxLength(20, ErrorMessage = "El titulo del evento no puede exceder los 20 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El titulo del evento debe contener letras y no puede ser solo números o símbolos")]
        [Required(ErrorMessage = "Se requiere un titulo para el evento")]
        public string Titulo { get; set; } = string.Empty;

        [MinLength(3, ErrorMessage = "El nombre del lugar debe tener al menos 3 caracteres")]
        [MaxLength(40, ErrorMessage = "El nombre del lugar no puede exceder los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El nombre del lugar debe contener letras y no puede ser solo números o símbolos")]
        [Required(ErrorMessage = "Se requiere un lugar para el evento")]
        public string Lugar { get; set; } = string.Empty;

        [MinLength(3, ErrorMessage = "El nombre de la calle debe tener al menos 3 caracteres")]
        [MaxLength(40, ErrorMessage = "El nombre de la calle no puede exceder los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "El nombre de la calle debe contener letras y no puede ser solo números o símbolos")]
        [Required(ErrorMessage = "Asigne la calle")]
        public string Calle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Asigne la altura")]
        public int? Altura { get; set; }
        public MudColor Color { get; set; } = new MudColor("#2196F3");

        [Required(ErrorMessage = "Asigne una fecha al evento")]
        [NotPastDate]
        public DateTime? Fecha { get; set; }
        public TimeSpan? HoraInicio { get; set; } 
        public TimeSpan? HoraFin { get; set; }
        public List<RolDTO> Roles { get; set; } = new();
        public static EventoViewModel ToEventoCustom(EventoDTO dto)
        {
            return new EventoViewModel
            {
                IdEvento = dto.IdEvento,
                Titulo = dto.Titulo,
                Fecha = dto.Fecha,
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
                Lugar = dto.Lugar,
                Calle = dto.Calle,
                Altura = dto.Altura,
                Color = new MudColor(dto.Color),
                AllDay = dto.Duracion_completa,
                Start = dto.Fecha.Value.Add(dto.HoraInicio.Value),
                End = dto.Fecha.Value.Add(dto.HoraFin.Value),
                Roles = dto.Roles
            };
        }

        public EventoDTO ToDTO()
        {
            return new EventoDTO
            {
                IdEvento = this.IdEvento,
                Titulo = this.Titulo,
                Fecha = this.Fecha,
                HoraInicio = this.HoraInicio,
                HoraFin = this.HoraFin,
                Lugar = this.Lugar,
                Calle = this.Calle,
                Altura = this.Altura,
                Color = this.Color.Value,
                Duracion_completa = this.AllDay,
                Roles = this.Roles
            };
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!AllDay)
            {
                if (Fecha != null && HoraInicio != null)
                {
                    if(Fecha.Value.Date == DateTime.Today && HoraInicio.Value < DateTime.Now.TimeOfDay)
                    {
                        yield return new ValidationResult(
                            "La hora de inicio no puede ser una hora pasada para la fecha actual.", new[] { nameof(HoraInicio) }
                        );
                    }
                }

                if (HoraInicio == null || HoraFin == null)
                {
                    yield return new ValidationResult("Debe indicar una hora si no dura todo el dia (ej -> 00:00)", new[] { nameof(HoraInicio), nameof(HoraFin) });
                }

                if (HoraInicio != null && HoraFin != null && HoraFin <= HoraInicio)
                {
                    yield return new ValidationResult("La hora de fin debe ser posterior a la hora de inicio", new[] { nameof(HoraFin) });
                }
            }
        }
    }
}
