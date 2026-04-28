using SistemaMPN.Modules.Autentificacion.Models;

namespace SistemaMPN.Modules.Autentificacion.Services
{
    public interface IEmailQueueService
    {
        Task EnqueueAsync(EmailRequest request);
        Task EnqueueBulkAsync(BulkEmailRequest request);
    }
}
