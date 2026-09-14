namespace CommUnityHub.Models
{
    public class HomeViewModel
    {
        public IEnumerable<ServiceCategory> FeaturedCategories { get; set; } = new List<ServiceCategory>();
        public IEnumerable<ServiceListing> RecentListings { get; set; } = new List<ServiceListing>();
        public int TotalListingsCount { get; set; }
        public int TotalCategoriesCount { get; set; }
    }
}
