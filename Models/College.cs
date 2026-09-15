using System.ComponentModel.DataAnnotations;

namespace FacultyInformationSystem_FIS_.Models
{
    public class College : BaseEntity
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = "";

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
