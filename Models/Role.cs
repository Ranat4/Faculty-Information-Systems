using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class Role : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        [MaxLength(200)]
        public string? Description { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RoleAccess> RoleAccesses { get; set; } = new List<RoleAccess>();
    }

    public enum AccessType
    {
        ChangePassword,
        Research,
        AnnualEvaluation,
        Manage
    }

    public enum Modules
    {
        SystemSetup
    }

    public class RoleAccess : BaseEntity
    {
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public Modules Module { get; set; }

        public AccessType Access { get; set; }
    }
}