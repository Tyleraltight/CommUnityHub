using Microsoft.EntityFrameworkCore;
using CommUnityHub.Data;
using CommUnityHub.Models;

namespace CommUnityHub.Repositories
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task UpdateStatusAsync(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = await _context.OrderHeaders.FirstOrDefaultAsync(u => u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public async Task UpdateStripePaymentIdAsync(int id, string sessionId, string? paymentIntentId)
        {
            var orderFromDb = await _context.OrderHeaders.FirstOrDefaultAsync(u => u.Id == id);
            if (orderFromDb != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    orderFromDb.SessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    orderFromDb.PaymentIntentId = paymentIntentId;
                    orderFromDb.PaymentDate = DateTime.Now;
                }
            }
        }
    }
}
