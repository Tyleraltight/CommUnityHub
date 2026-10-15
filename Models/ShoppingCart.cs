using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommUnityHub.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        public int ServiceListingId { get; set; }

        [ForeignKey("ServiceListingId")]
        [ValidateNever]
        public ServiceListing? ServiceListing { get; set; }

        [Range(1, 100, ErrorMessage = "Please enter a value between 1 and 100")]
        public int Count { get; set; } = 1;

        public string ApplicationUserId { get; set; } = string.Empty;

        [NotMapped]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        [NotMapped]
        public decimal Price { get; set; }
    }
}
