using Microsoft.EntityFrameworkCore;
using FacultyInformationSystem_FIS_.Models;

namespace FacultyInformationSystem_FIS_.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DemoRequest> DemoRequests => Set<DemoRequest>();

        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    }
}
