using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using SistemaMPN.Modules.Autentificacion.Models;

namespace SistemaMPN.Modules.Autentificacion.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailQueueService _queueService;

        public EmailService(IConfiguration configuration, IEmailQueueService queueService)
        {
            _configuration = configuration;
            _queueService = queueService;
        }

        public async Task<string> SendEmailAsync(EmailRequest request)
        {
            await _queueService.EnqueueAsync(request);
            return "Email encolado para envío";
        }

        public async Task<string> SendBulkEmailAsync(BulkEmailRequest request)
        {
            await _queueService.EnqueueBulkAsync(request);
            return $"Emails encolados para {request.Recipients?.Count ?? 0} destinatarios";
        }
    }
}
