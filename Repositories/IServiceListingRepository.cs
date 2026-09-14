using CommUnityHub.Models;

namespace CommUnityHub.Repositories
{
    public interface IServiceListingRepository : IRepository<ServiceListing>
    {
        Task<IEnumerable<ServiceListing>> GetListingsWithCategoryAsync();
        Task<ServiceListing?> GetListingWithCategoryByIdAsync(int id);
        Task<IEnumerable<ServiceListing>> FilterListingsAsync(int? categoryId, string? suburb, string? searchTerm);
    }
}
