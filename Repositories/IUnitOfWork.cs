using CommUnityHub.Models;

namespace CommUnityHub.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<ServiceCategory> Categories { get; }
        IServiceListingRepository ServiceListings { get; }
        IRepository<ShoppingCart> ShoppingCarts { get; }
        IOrderHeaderRepository OrderHeaders { get; }
        IRepository<OrderDetail> OrderDetails { get; }
        IRepository<ApplicationUser> ApplicationUsers { get; }
        Task<int> CompleteAsync();
    }
}
