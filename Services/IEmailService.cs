namespace FacultyInformationSystem_FIS_.Services
{
    public interface IEmailService
    {
        // Sends a plain-text email using the SMTP settings from appsettings.json.
        // replyToEmail is set so that replying to the notification email goes
        // straight back to whoever filled out the form, not to yourself.
        Task SendAsync(string subject, string body, string replyToEmail, string replyToName);
    }
}
