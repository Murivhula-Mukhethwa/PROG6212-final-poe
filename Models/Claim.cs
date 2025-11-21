using System;
using System.ComponentModel.DataAnnotations;

namespace CMCS.web2.Models
{
    public class Claim
    {
        public Guid ClaimId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; } // Link to user

        // Remove manual input fields, get from user profile
        public string LecturerEmail { get; set; } = string.Empty;
        public string LecturerName { get; set; } = string.Empty;

        [Required]
        [Range(0.25, 180, ErrorMessage = "Hours must be between 0.25 and 180 per month")] // Updated validation
        public double HoursWorked { get; set; }

        public decimal HourlyRate { get; set; } // From user profile

        public string? Notes { get; set; }

        // File metadata
        public string? OriginalFileName { get; set; }
        public string? StoredFileName { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdated { get; set; }
        public string? LastUpdatedBy { get; set; }

        // Calculated property
        public decimal TotalAmount => (decimal)HoursWorked * HourlyRate;
    }
}