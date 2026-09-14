using System.ComponentModel.DataAnnotations;

namespace CommUnityHub.Models
{
    public class ServiceCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string? Description { get; set; }

        [Display(Name = "Icon Class")]
        [StringLength(50)]
        public string? IconClass { get; set; }

        [Display(Name = "Display Order")]
        [Range(1, 100, ErrorMessage = "Display order must be between 1 and 100")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public ICollection<ServiceListing> ServiceListings { get; set; } = new List<ServiceListing>();
    }
}
