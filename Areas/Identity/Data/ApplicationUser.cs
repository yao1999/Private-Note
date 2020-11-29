using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Private_Note.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    { 
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        [Display(Name = "Password")]
        [MaxLength(30)]
        public string Password { get; set; }
        [PersonalData]
        [Column(TypeName = "nvarchar(200)")]
        [Display(Name = "Secret Password")]
        [MaxLength(16)]
        public string SecretPassword { get; set; }
        public bool IsUser { get; set; }
        public bool IsAdmin { get; set; }
    }
}
