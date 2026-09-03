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
        public DbSet<Degree> Degrees => Set<Degree>();

        public DbSet<CvRecord> CvRecords => Set<CvRecord>();
        public DbSet<Notification> Notifications => Set<Notification>();

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

            modelBuilder.Entity<RoleAccess>()
               .HasIndex(ra => new { ra.RoleId, ra.Module, ra.Access })
               .IsUnique();

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "System administrator" },
                new Role { Id = 2, Name = "Faculty", Description = "Faculty member" },
                new Role { Id = 4, Name = "Department Chair", Description = "Faculty member who also heads a department" },
                new Role { Id = 5, Name = "Dean", Description = "Head of a college/school" }
            );

            modelBuilder.Entity<RoleAccess>().HasData(
                new RoleAccess { Id = 8, RoleId = 1, Module = Modules.SystemSetup, Access = AccessType.Manage },
                new RoleAccess { Id = 9, RoleId = 1, Module = Modules.DemoRequests, Access = AccessType.Manage },
                new RoleAccess { Id = 10, RoleId = 1, Module = Modules.ContactRequests, Access = AccessType.Manage },
                new RoleAccess { Id = 11, RoleId = 1, Module = Modules.Users, Access = AccessType.Manage },
                new RoleAccess { Id = 12, RoleId = 1, Module = Modules.RoleManagement, Access = AccessType.Manage }
            );
        }
    }
}
