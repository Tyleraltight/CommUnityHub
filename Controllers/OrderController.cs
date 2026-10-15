using CommUnityHub.Models;
using CommUnityHub.Models.ViewModels;
using CommUnityHub.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CommUnityHub.Controllers
{
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; } = new();

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var orders = (await _unitOfWork.OrderHeaders.GetAllAsync()).OrderByDescending(u => u.OrderDate).ToList();

            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                orders = status.ToLower() switch
                {
                    "pending" => orders.Where(u => u.PaymentStatus == SD.PaymentStatusPending).ToList(),
                    "approved" => orders.Where(u => u.OrderStatus == SD.StatusApproved).ToList(),
                    "processing" => orders.Where(u => u.OrderStatus == SD.StatusInProcess).ToList(),
                    "shipped" => orders.Where(u => u.OrderStatus == SD.StatusShipped).ToList(),
                    "cancelled" => orders.Where(u => u.OrderStatus == SD.StatusCancelled).ToList(),
                    _ => orders
                };
            }

            ViewData["CurrentStatus"] = status ?? "all";
            return View(orders);
        }

        public async Task<IActionResult> Details(int orderId)
        {
            var orderHeader = await _unitOfWork.OrderHeaders.GetByIdAsync(orderId);
            if (orderHeader == null)
            {
                return NotFound();
            }

            var orderDetails = (await _unitOfWork.OrderDetails.FindAsync(u => u.OrderHeaderId == orderId)).ToList();
            foreach (var detail in orderDetails)
            {
                detail.ServiceListing = await _unitOfWork.ServiceListings.GetListingWithCategoryByIdAsync(detail.ServiceListingId);
            }

            OrderVM = new OrderVM
            {
                OrderHeader = orderHeader,
                OrderDetails = orderDetails
            };

            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderDetail()
        {
            var orderHeaderFromDb = await _unitOfWork.OrderHeaders.GetByIdAsync(OrderVM.OrderHeader.Id);
            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
                orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
                orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
                orderHeaderFromDb.City = OrderVM.OrderHeader.City;
                orderHeaderFromDb.Suburb = OrderVM.OrderHeader.Suburb;
                orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

                if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
                {
                    orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
                }
                if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
                {
                    orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
                }

                _unitOfWork.OrderHeaders.Update(orderHeaderFromDb);
                await _unitOfWork.CompleteAsync();

                TempData["Success"] = "Order contact details updated successfully.";
                return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
            }

            TempData["Error"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartProcessing()
        {
            await _unitOfWork.OrderHeaders.UpdateStatusAsync(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            await _unitOfWork.CompleteAsync();

            TempData["Success"] = "Order status updated to Processing.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShipOrder()
        {
            var orderHeader = await _unitOfWork.OrderHeaders.GetByIdAsync(OrderVM.OrderHeader.Id);
            if (orderHeader != null)
            {
                orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber ?? "TRK-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
                orderHeader.Carrier = OrderVM.OrderHeader.Carrier ?? "NZ Post / Community Courier";
                orderHeader.OrderStatus = SD.StatusShipped;
                orderHeader.ShippingDate = DateTime.Now;

                _unitOfWork.OrderHeaders.Update(orderHeader);
                await _unitOfWork.CompleteAsync();

                TempData["Success"] = "Order marked as Shipped/Dispatched with tracking information.";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }

            TempData["Error"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder()
        {
            var orderHeader = await _unitOfWork.OrderHeaders.GetByIdAsync(OrderVM.OrderHeader.Id);
            if (orderHeader != null)
            {
                if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
                {
                    await _unitOfWork.OrderHeaders.UpdateStatusAsync(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
                }
                else
                {
                    await _unitOfWork.OrderHeaders.UpdateStatusAsync(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
                }
                await _unitOfWork.CompleteAsync();

                TempData["Success"] = "Order cancelled successfully.";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }

            TempData["Error"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }
    }
}
