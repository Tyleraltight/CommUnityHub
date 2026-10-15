using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommUnityHub.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;

        [NotMapped]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ShippingDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }

        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }

        public DateTime? PaymentDate { get; set; }
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required")]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        public string? Suburb { get; set; }

        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }
    }
}
