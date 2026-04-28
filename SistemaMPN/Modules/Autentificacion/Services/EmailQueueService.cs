using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using SistemaMPN.Modules.Autentificacion.Models;
using System.Threading.Channels;

namespace SistemaMPN.Modules.Autentificacion.Services
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly Channel<EmailRequest> _emailChannel;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailQueueService> _logger;
        private readonly CancellationTokenSource _cts;

        public EmailQueueService(IConfiguration configuration, ILogger<EmailQueueService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _cts = new CancellationTokenSource();

            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _emailChannel = Channel.CreateBounded<EmailRequest>(options);

            _ = ProcessQueueAsync();
        }

        public async Task EnqueueAsync(EmailRequest request)
        {
            await _emailChannel.Writer.WriteAsync(request);
        }

        public async Task EnqueueBulkAsync(BulkEmailRequest request)
        {
            if (request.Recipients == null || !request.Recipients.Any())
            {
                return;
            }

            foreach (var recipient in request.Recipients)
            {
                var emailRequest = new EmailRequest
                {
                    To = recipient,
                    Subject = request.Subject,
                    Message = request.Message
                };
                await _emailChannel.Writer.WriteAsync(emailRequest);
            }
        }

        private async Task ProcessQueueAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var email = await _emailChannel.Reader.ReadAsync(_cts.Token);

                    try
                    {
                        await SendEmailInternalAsync(email);
                        _logger.LogInformation("Email sent to {To}", email.To);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception sending email to {To}", email.To);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email queue");
                }
            }
        }

        private async Task SendEmailInternalAsync(EmailRequest request)
        {
            using var smtp = new SmtpClient();
            smtp.Connect(_configuration["EmailSettings:EmailHost"], 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration["EmailSettings:EmailUsername"], _configuration["EmailSettings:EmailPassword"]);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:EmailUsername"]));
            email.To.Add(MailboxAddress.Parse(request.To));

            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Message };

            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        public void Dispose()
        {
            _emailChannel.Writer.Complete();
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
