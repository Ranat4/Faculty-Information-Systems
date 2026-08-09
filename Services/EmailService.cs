using System.Net;
using System.Net.Mail;
using FacultyInformationSystem_FIS_.Models;
using Microsoft.Extensions.Options;

namespace FacultyInformationSystem_FIS_.Services
{
    // Uses System.Net.Mail.SmtpClient — the built-in .NET mail client.
    // Microsoft marks SmtpClient as "not recommended" for large-scale
    // production use and suggests MailKit instead, but it requires no
    // extra NuGet package, which makes it the simpler choice while
    // you're still learning the project structure. Swapping to MailKit
    // later only means changing what happens inside this one class —
    // nothing else in the project would need to change.
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string subject, string body, string replyToEmail, string replyToName)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
            message.To.Add(_settings.RecipientEmail);
            message.ReplyToList.Add(new MailAddress(replyToEmail, replyToName));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = false;

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
                EnableSsl = _settings.EnableSsl
            };

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Logged, not thrown further as-is, so a broken SMTP config
                // doesn't crash the page — the controller decides what the
                // user sees when this fails (see HomeController).
                _logger.LogError(ex, "Failed to send email via SMTP.");
                throw;
            }
        }
    }
}
