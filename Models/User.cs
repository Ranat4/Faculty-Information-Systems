using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class User : BaseEntity
    {
        [Required, MaxLength(200)]
        public string FullName { get; set; } = "";

        [Required, MaxLength(200)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public int? CollegeId { get; set; }
        public College? College { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserRole : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}
