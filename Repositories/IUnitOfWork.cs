using CommUnityHub.Models;

namespace CommUnityHub.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<ServiceCategory> Categories { get; }
        IServiceListingRepository ServiceListings { get; }
        Task<int> CompleteAsync();
    }
}
