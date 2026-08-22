using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class PasswordResetCode
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Email { get; set; } = "";

        [Required, MaxLength(10)]
        public string Code { get; set; } = "";

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
