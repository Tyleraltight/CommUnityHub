using CommUnityHub.Data;
using CommUnityHub.Models;

namespace CommUnityHub.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<ServiceCategory>? _categories;
        private IServiceListingRepository? _serviceListings;
        private IRepository<ShoppingCart>? _shoppingCarts;
        private IOrderHeaderRepository? _orderHeaders;
        private IRepository<OrderDetail>? _orderDetails;
        private IRepository<ApplicationUser>? _applicationUsers;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<ServiceCategory> Categories
        {
            get
            {
                _categories ??= new Repository<ServiceCategory>(_context);
                return _categories;
            }
        }

        public IServiceListingRepository ServiceListings
        {
            get
            {
                _serviceListings ??= new ServiceListingRepository(_context);
                return _serviceListings;
            }
        }

        public IRepository<ShoppingCart> ShoppingCarts
        {
            get
            {
                _shoppingCarts ??= new Repository<ShoppingCart>(_context);
                return _shoppingCarts;
            }
        }

        public IOrderHeaderRepository OrderHeaders
        {
            get
            {
                _orderHeaders ??= new OrderHeaderRepository(_context);
                return _orderHeaders;
            }
        }

        public IRepository<OrderDetail> OrderDetails
        {
            get
            {
                _orderDetails ??= new Repository<OrderDetail>(_context);
                return _orderDetails;
            }
        }

        public IRepository<ApplicationUser> ApplicationUsers
        {
            get
            {
                _applicationUsers ??= new Repository<ApplicationUser>(_context);
                return _applicationUsers;
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
