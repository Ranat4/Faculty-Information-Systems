using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        [MaxLength(200)]
        public string? Description { get; set; }


        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<RoleAccess> RoleAccesses { get; set; } = new List<RoleAccess>();
    }
}
