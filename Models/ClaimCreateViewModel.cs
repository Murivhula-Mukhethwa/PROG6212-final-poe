using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CMCS.web2.Models
{
    public class ClaimCreateViewModel
    {
        [Required(ErrorMessage = "Hours worked is required")]
        [Range(0.25, 180, ErrorMessage = "Hours must be between 0.25 and 180 per month")]
        [Display(Name = "Hours Worked")]
        public double HoursWorked { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Supporting document is required")]
        [Display(Name = "Supporting Document")]
        public IFormFile? SupportingDocument { get; set; }

        // These will be populated from user profile
        public string LecturerEmail { get; set; } = string.Empty;
        public string LecturerName { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
    }
}
