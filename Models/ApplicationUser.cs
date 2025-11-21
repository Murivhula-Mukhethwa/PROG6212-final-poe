using Microsoft.AspNetCore.Identity;
using System;

namespace CMCS.web2.Models
{
    public class ApplicationUser : IdentityUser
    {
        // optional link to Lecturer entity if this user is a Lecturer
        public Guid? LecturerId { get; set; }
    }
}
