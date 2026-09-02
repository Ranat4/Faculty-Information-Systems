using FacultyInformationSystem_FIS_.Models;

namespace FacultyInformationSystem_FIS_.Services
{
    public interface INotificationService
    {
        // Always creates an in-system Notification row. If sendEmail is
        // true, also emails the recipient via the existing IEmailService.
        Task NotifyAsync(User recipient, string message, string? actionUrl, bool sendEmail, string? emailSubject = null);
    }
}
