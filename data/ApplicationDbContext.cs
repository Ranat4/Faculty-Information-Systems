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
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrator" },
                new Role { Id = 2, Name = "Faculty" },
                new Role { Id = 3, Name = "Student" }
            );
        }
    }
}
