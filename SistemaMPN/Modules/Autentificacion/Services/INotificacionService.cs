using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Autentificacion.Services
{
    public interface INotificacionService
    {
        Task EnviarNotificacionPeticion(PeticionDTO peticion);

        Task EnviarNotificacionPropuesta(PropuestaDeCambioDTO propuesta);
    }
}
