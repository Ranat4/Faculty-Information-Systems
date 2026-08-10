namespace FacultyInformationSystem_FIS_.Services
{
    public interface IEmailService
    {
      
        Task SendAsync(string subject, string plainTextBody, string replyToEmail, string replyToName, string? htmlBody = null);
    }
}
