using Microsoft.EntityFrameworkCore;
using CommUnityHub.Data;
using CommUnityHub.Models;

namespace CommUnityHub.Repositories
{
    public class ServiceListingRepository : Repository<ServiceListing>, IServiceListingRepository
    {
        public ServiceListingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ServiceListing>> GetListingsWithCategoryAsync()
        {
            return await _context.ServiceListings
                .Include(l => l.Category)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<ServiceListing?> GetListingWithCategoryByIdAsync(int id)
        {
            return await _context.ServiceListings
                .Include(l => l.Category)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<ServiceListing>> FilterListingsAsync(int? categoryId, string? suburb, string? searchTerm)
        {
            var query = _context.ServiceListings.Include(l => l.Category).AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(l => l.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(suburb))
            {
                query = query.Where(l => l.Suburb.ToLower().Contains(suburb.Trim().ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(l => l.Title.ToLower().Contains(term) || l.Description.ToLower().Contains(term));
            }

            return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }
    }
}
