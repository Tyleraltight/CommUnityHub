using CommUnityHub.Data;
using CommUnityHub.Models;

namespace CommUnityHub.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<ServiceCategory>? _categories;
        private IServiceListingRepository? _serviceListings;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<ServiceCategory> Categories
        {
            get
            {
                if (_categories == null)
                {
                    _categories = new Repository<ServiceCategory>(_context);
                }
                return _categories;
            }
        }

        public IServiceListingRepository ServiceListings
        {
            get
            {
                if (_serviceListings == null)
                {
                    _serviceListings = new ServiceListingRepository(_context);
                }
                return _serviceListings;
            }
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
