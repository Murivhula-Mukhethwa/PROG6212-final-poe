using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.web2.Models
{
    public class Lecturer
    {
        [Key]
        public Guid LecturerId { get; set; } = Guid.NewGuid();

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        public bool IsApproved { get; set; } = false;

        // Navigation property for EF Core
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
