using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommUnityHub.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        [Required]
        public int OrderHeaderId { get; set; }

        [ForeignKey("OrderHeaderId")]
        [ValidateNever]
        public OrderHeader? OrderHeader { get; set; }

        [Required]
        public int ServiceListingId { get; set; }

        [ForeignKey("ServiceListingId")]
        [ValidateNever]
        public ServiceListing? ServiceListing { get; set; }

        public int Count { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
