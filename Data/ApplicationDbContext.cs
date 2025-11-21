using Microsoft.EntityFrameworkCore;
using CMCS.web2.Models;

namespace CMCS.web2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Claim> Claims { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Surname).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Password).IsRequired().HasMaxLength(255);
                entity.Property(u => u.HourlyRate).HasColumnType("decimal(18,2)");

                // Create index on email for performance
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Configure Claim entity
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasKey(c => c.ClaimId);
                entity.Property(c => c.LecturerEmail).IsRequired().HasMaxLength(255);
                entity.Property(c => c.LecturerName).IsRequired().HasMaxLength(200);
                entity.Property(c => c.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(c => c.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(c => c.OriginalFileName).HasMaxLength(500);
                entity.Property(c => c.StoredFileName).HasMaxLength(500);
                entity.Property(c => c.LastUpdatedBy).HasMaxLength(100);

                // Relationship with User
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed initial data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = Guid.NewGuid(),
                    Email = "hr@university.com",
                    Name = "HR",
                    Surname = "Admin",
                    HourlyRate = 0,
                    Password = "hr123",
                    Role = UserRole.HR,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Email = "lecturer@university.com",
                    Name = "John",
                    Surname = "Doe",
                    HourlyRate = 50.00m,
                    Password = "lecturer123",
                    Role = UserRole.Lecturer,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Email = "coordinator@university.com",
                    Name = "Jane",
                    Surname = "Smith",
                    HourlyRate = 0,
                    Password = "coordinator123",
                    Role = UserRole.ProgrammeCoordinator,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Email = "manager@university.com",
                    Name = "Robert",
                    Surname = "Johnson",
                    HourlyRate = 0,
                    Password = "manager123",
                    Role = UserRole.AcademicManager,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }
            );
        }
    }
}