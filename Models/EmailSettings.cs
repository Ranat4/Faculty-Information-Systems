namespace FacultyInformationSystem_FIS_.Models
{
    // Mirrors the "EmailSettings" section in appsettings.json.
    // Bound automatically via builder.Services.Configure<EmailSettings>(...)
    // in Program.cs — see EmailService for where this actually gets used.
    public class EmailSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string SenderEmail { get; set; } = "";
        public string SenderPassword { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string RecipientEmail { get; set; } = "";
    }
}
