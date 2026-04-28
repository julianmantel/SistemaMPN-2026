using SistemaMPN.Modules.Autentificacion.Models;

namespace SistemaMPN.Modules.Autentificacion.Services
{
    public interface IEmailService
    {
        Task<string> SendEmailAsync(EmailRequest request);
        Task<string> SendBulkEmailAsync(BulkEmailRequest request);
    }
}
