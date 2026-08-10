using System.Net;
using System.Net.Mail;
using FacultyInformationSystem_FIS_.Models;
using Microsoft.Extensions.Options;

namespace FacultyInformationSystem_FIS_.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string subject, string plainTextBody, string replyToEmail, string replyToName, string? htmlBody = null)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.FromAddress, _settings.FromName);
            message.To.Add("emreakbas042@gmail.com");
            message.ReplyToList.Add(new MailAddress(replyToEmail, replyToName));
            message.Subject = subject;

            if (htmlBody is not null)
            {
                var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
                message.AlternateViews.Add(plainView);
                message.AlternateViews.Add(htmlView);
            }
            else
            {
                message.Body = plainTextBody;
                message.IsBodyHtml = false;
            }

            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl
            };

            if (_settings.UseCredentials)
            {
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }

            try
            {
                await client.SendMailAsync(message);

                if (_settings.MailDelay > 0)
                {
                    await Task.Delay(_settings.MailDelay);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SMTP.");
                throw;
            }
        }
    }
}
