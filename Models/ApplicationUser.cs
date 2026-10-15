using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CommUnityHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Street Address")]
        public string? StreetAddress { get; set; }

        public string? City { get; set; }

        public string? Suburb { get; set; }

        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }
    }
}
