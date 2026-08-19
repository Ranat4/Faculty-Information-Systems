using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = "";

        [Required, MaxLength(200)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
