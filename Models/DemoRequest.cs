using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class DemoRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = "";

        [Required, MaxLength(200)]
        public string Email { get; set; } = "";

        [Required, MaxLength(200)]
        public string Institution { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
