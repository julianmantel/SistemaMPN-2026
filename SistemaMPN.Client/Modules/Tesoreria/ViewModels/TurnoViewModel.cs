using Heron.MudCalendar;
using SistemaMPN.Shared.DTO;
using System.ComponentModel.DataAnnotations;

namespace SistemaMPN.Client.Modules.Tesoreria.ViewModels
{
    public class TurnoViewModel : CalendarItem
    {
        [ValidateComplexType]
        public TurnoDTO turnoDTO { get; set; } = new();
    }
}
