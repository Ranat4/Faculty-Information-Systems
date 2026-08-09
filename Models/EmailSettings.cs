namespace FacultyInformationSystem_FIS_.Models
{
    public class EmailSettings
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool IsEncrypted { get; set; }
        public string SmtpServer { get; set; } = "";
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseCredentials { get; set; }
        public string FromName { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public int MailDelay { get; set; }
    }
}
