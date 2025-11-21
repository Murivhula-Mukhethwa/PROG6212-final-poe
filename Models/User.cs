using System.ComponentModel.DataAnnotations;

namespace CMCS.web2.Models
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 1000000)]
        public decimal HourlyRate { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty; // Hashed in production

        public UserRole Role { get; set; } = UserRole.Lecturer;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public enum UserRole
    {
        HR,
        Lecturer,
        ProgrammeCoordinator,
        AcademicManager
    }
}