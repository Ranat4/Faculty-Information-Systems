namespace FacultyInformationSystem_FIS_.Services
{
    public interface IEmailService
    {
        // recipientEmail defaults to null, which means "send to the
        // configured FromAddress" (used by Contact/Demo Request — admin
        // gets notified). Pass an explicit recipientEmail to send to a
        // specific person instead (used by password reset codes).
        Task SendAsync(string subject, string plainTextBody, string replyToEmail, string replyToName, string? htmlBody = null, string? recipientEmail = null);
    }
}
