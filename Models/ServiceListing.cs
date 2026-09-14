using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommUnityHub.Models
{
    public class ServiceListing
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public ServiceCategory? Category { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be at least 10 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Provider name is required")]
        [StringLength(50, ErrorMessage = "Provider name cannot exceed 50 characters")]
        [Display(Name = "Provider Name")]
        public string ProviderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string ContactEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suburb is required")]
        [StringLength(50, ErrorMessage = "Suburb cannot exceed 50 characters")]
        public string Suburb { get; set; } = string.Empty;

        [Required(ErrorMessage = "Exchange type is required")]
        [Display(Name = "Exchange Type")]
        public string ExchangeType { get; set; } = "Free Skill Swap";

        [Display(Name = "Deposit Required?")]
        public bool DepositRequired { get; set; } = false;

        [Display(Name = "Estimated Material / Deposit ($)")]
        [Range(0, 1000, ErrorMessage = "Cost must be between 0 and 1000")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedCost { get; set; } = 0.00m;

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Created Date")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
