using FacultyInformationSystem_FIS_.Data;
using FacultyInformationSystem_FIS_.Models;

namespace FacultyInformationSystem_FIS_.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public NotificationService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task NotifyAsync(User recipient, string message, string? actionUrl, bool sendEmail, string? emailSubject = null)
        {
            var notification = new Notification
            {
                UserId = recipient.Id,
                Message = message,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            if (sendEmail)
            {
                await _emailService.SendAsync(
                    subject: emailSubject ?? "New notification",
                    plainTextBody: message,
                    replyToEmail: recipient.Email,
                    replyToName: recipient.FullName,
                    recipientEmail: recipient.Email);
            }
        }
    }
}
