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
        public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RoleAccess> RoleAccesses => Set<RoleAccess>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<RoleAccess>()
                .HasOne(ra => ra.Role)
                .WithMany(r => r.RoleAccesses)
                .HasForeignKey(ra => ra.RoleId);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrator", Description = "System administrator" },
                new Role { Id = 3, Name = "Student", Description = "Student user" }
            );
        }
    }
}
