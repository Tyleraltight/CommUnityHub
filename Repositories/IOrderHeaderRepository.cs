using CommUnityHub.Models;

namespace CommUnityHub.Repositories
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        Task UpdateStatusAsync(int id, string orderStatus, string? paymentStatus = null);
        Task UpdateStripePaymentIdAsync(int id, string sessionId, string? paymentIntentId);
    }
}
