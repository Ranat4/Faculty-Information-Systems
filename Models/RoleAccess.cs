namespace FacultyInformationSystem_FIS_.Models
{
    public class RoleAccess
    {
        public int ID { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public AccessType Access {  get; set; }
    }
}
