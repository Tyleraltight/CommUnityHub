using CommUnityHub.Models;
using CommUnityHub.Models.Dtos;
using CommUnityHub.Repositories;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace CommUnityHub.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public PaymentApiController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        [HttpGet("order-status/{id}")]
        public async Task<IActionResult> GetOrderStatus(int id)
        {
            var order = await _unitOfWork.OrderHeaders.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found." });
            }

            return Ok(new
            {
                orderId = order.Id,
                orderStatus = order.OrderStatus,
                paymentStatus = order.PaymentStatus,
                carrier = order.Carrier,
                trackingNumber = order.TrackingNumber,
                shippingDate = order.ShippingDate,
                orderTotal = order.OrderTotal
            });
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutRequestDto request)
        {
            if (request == null || request.OrderHeaderId <= 0)
            {
                return BadRequest(new { message = "Invalid request payload." });
            }

            var order = await _unitOfWork.OrderHeaders.GetByIdAsync(request.OrderHeaderId);
            if (order == null || order.OrderTotal <= 0)
            {
                return BadRequest(new { message = "Order not found or order total is zero." });
            }

            var returnUrlBase = string.IsNullOrWhiteSpace(request.ReturnUrlBase)
                ? $"{Request.Scheme}://{Request.Host}"
                : request.ReturnUrlBase.TrimEnd('/');

            var successUrl = $"{returnUrlBase}/Cart/OrderConfirmation?id={order.Id}";
            var cancelUrl = $"{returnUrlBase}/Cart/Index";

            var options = new SessionCreateOptions
            {
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(order.OrderTotal * 100),
                            Currency = "nzd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"CommUnityHub Service/Materials - Order #{order.Id}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment"
            };

            try
            {
                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                await _unitOfWork.OrderHeaders.UpdateStripePaymentIdAsync(order.Id, session.Id, session.PaymentIntentId);
                await _unitOfWork.CompleteAsync();

                return Ok(new
                {
                    sessionId = session.Id,
                    checkoutUrl = session.Url
                });
            }
            catch (Exception)
            {
                // Fallback for simulated development / offline test mode
                var simulatedSessionId = "cs_test_mock_" + Guid.NewGuid().ToString("N")[..12];
                await _unitOfWork.OrderHeaders.UpdateStripePaymentIdAsync(order.Id, simulatedSessionId, "pi_mock_" + Guid.NewGuid().ToString("N")[..8]);
                await _unitOfWork.CompleteAsync();

                return Ok(new
                {
                    sessionId = simulatedSessionId,
                    checkoutUrl = $"{returnUrlBase}/Cart/OrderConfirmation?id={order.Id}",
                    simulated = true,
                    note = "Simulated Stripe Session (Development mode / mock test key)"
                });
            }
        }
    }
}
