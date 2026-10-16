using CommUnityHub.Models;
using CommUnityHub.Models.ViewModels;
using CommUnityHub.Repositories;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace CommUnityHub.Controllers
{
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; } = new();

        public CartController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        private string GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null) return claim.Value;
            }

            var guestId = HttpContext.Session.GetString("GuestUserId");
            if (string.IsNullOrEmpty(guestId))
            {
                guestId = "guest_" + Guid.NewGuid().ToString("N")[..8];
                HttpContext.Session.SetString("GuestUserId", guestId);
            }
            return guestId;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var cartList = (await _unitOfWork.ShoppingCarts.FindAsync(u => u.ApplicationUserId == userId)).ToList();

            foreach (var cart in cartList)
            {
                cart.ServiceListing = await _unitOfWork.ServiceListings.GetListingWithCategoryByIdAsync(cart.ServiceListingId);
                cart.Price = cart.ServiceListing != null ? cart.ServiceListing.EstimatedCost : 0;
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            ShoppingCartVM.ShoppingCartList = cartList;
            return View(ShoppingCartVM);
        }

        public async Task<IActionResult> AddToCart(int listingId, int count = 1)
        {
            var userId = GetCurrentUserId();
            var existingCart = (await _unitOfWork.ShoppingCarts.FindAsync(
                u => u.ApplicationUserId == userId && u.ServiceListingId == listingId)).FirstOrDefault();

            if (existingCart == null)
            {
                var cart = new ShoppingCart
                {
                    ServiceListingId = listingId,
                    ApplicationUserId = userId,
                    Count = count > 0 ? count : 1
                };
                await _unitOfWork.ShoppingCarts.AddAsync(cart);
            }
            else
            {
                existingCart.Count += count;
                _unitOfWork.ShoppingCarts.Update(existingCart);
            }

            await _unitOfWork.CompleteAsync();

            var totalItems = (await _unitOfWork.ShoppingCarts.FindAsync(u => u.ApplicationUserId == userId)).Sum(c => c.Count);
            HttpContext.Session.SetInt32(SD.SessionCart, totalItems);

            TempData["Success"] = "Item added to cart successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _unitOfWork.ShoppingCarts.GetByIdAsync(cartId);
            if (cart != null)
            {
                cart.Count += 1;
                _unitOfWork.ShoppingCarts.Update(cart);
                await _unitOfWork.CompleteAsync();

                var userId = GetCurrentUserId();
                var totalItems = (await _unitOfWork.ShoppingCarts.FindAsync(u => u.ApplicationUserId == userId)).Sum(c => c.Count);
                HttpContext.Session.SetInt32(SD.SessionCart, totalItems);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _unitOfWork.ShoppingCarts.GetByIdAsync(cartId);
            if (cart != null)
            {
                var userId = GetCurrentUserId();
                if (cart.Count <= 1)
                {
                    _unitOfWork.ShoppingCarts.Remove(cart);
                }
                else
                {
                    cart.Count -= 1;
                    _unitOfWork.ShoppingCarts.Update(cart);
                }
                await _unitOfWork.CompleteAsync();

                var totalItems = (await _unitOfWork.ShoppingCarts.FindAsync(u => u.ApplicationUserId == userId)).Sum(c => c.Count);
                HttpContext.Session.SetInt32(SD.SessionCart, totalItems);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _unitOfWork.ShoppingCarts.GetByIdAsync(cartId);
            if (cart != null)
            {
                var userId = GetCurrentUserId();
                _unitOfWork.ShoppingCarts.Remove(cart);
                await _unitOfWork.CompleteAsync();

                var totalItems = (await _unitOfWork.ShoppingCarts.FindAsync(u => u.ApplicationUserId == userId)).Sum(c => c.Count);
                HttpContext.Session.SetInt32(SD.SessionCart, totalItems);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Summary()
        {
            var userId = GetCurrentUserId();
            var cartList = (await _unitOfWork.ShoppingCarts.FindAsync(u => u.ApplicationUserId == userId)).ToList();

            if (!cartList.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items before checking out.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var cart in cartList)
            {
                cart.ServiceListing = await _unitOfWork.ServiceListings.GetListingWithCategoryByIdAsync(cart.ServiceListingId);
                cart.Price = cart.ServiceListing != null ? cart.ServiceListing.EstimatedCost : 0;
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            ShoppingCartVM.ShoppingCartList = cartList;
            ShoppingCartVM.OrderHeader.Name = User.Identity?.Name ?? "Community Resident";
            ShoppingCartVM.OrderHeader.PhoneNumber = "021-000-0000";
            ShoppingCartVM.OrderHeader.StreetAddress = "123 Community Way";
            ShoppingCartVM.OrderHeader.City = "Lower Hutt";
            ShoppingCartVM.OrderHeader.Suburb = "Petone";
            ShoppingCartVM.OrderHeader.PostalCode = "5012";

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPOST(ShoppingCartVM shoppingCartVM)
        {
            var userId = GetCurrentUserId();
            var cartList = (await _unitOfWork.ShoppingCarts.FindAsync(u => u.ApplicationUserId == userId)).ToList();

            if (!cartList.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            decimal total = 0;
            foreach (var cart in cartList)
            {
                cart.ServiceListing = await _unitOfWork.ServiceListings.GetListingWithCategoryByIdAsync(cart.ServiceListingId);
                cart.Price = cart.ServiceListing != null ? cart.ServiceListing.EstimatedCost : 0;
                total += (cart.Price * cart.Count);
            }

            var orderHeader = shoppingCartVM.OrderHeader;
            orderHeader.ApplicationUserId = userId;
            orderHeader.OrderDate = DateTime.Now;
            orderHeader.OrderTotal = total;
            orderHeader.OrderStatus = SD.StatusPending;
            orderHeader.PaymentStatus = SD.PaymentStatusPending;

            await _unitOfWork.OrderHeaders.AddAsync(orderHeader);
            await _unitOfWork.CompleteAsync();

            foreach (var item in cartList)
            {
                var orderDetail = new OrderDetail
                {
                    OrderHeaderId = orderHeader.Id,
                    ServiceListingId = item.ServiceListingId,
                    Price = item.Price,
                    Count = item.Count
                };
                await _unitOfWork.OrderDetails.AddAsync(orderDetail);
            }
            await _unitOfWork.CompleteAsync();

            // Stripe Checkout Session Creation
            try
            {
                var domain = $"{Request.Scheme}://{Request.Host}/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Cart/OrderConfirmation?id={orderHeader.Id}",
                    CancelUrl = domain + "Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in cartList)
                {
                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "nzd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.ServiceListing?.Title ?? "Community Service Booking"
                            }
                        },
                        Quantity = item.Count
                    });
                }

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                await _unitOfWork.OrderHeaders.UpdateStripePaymentIdAsync(orderHeader.Id, session.Id, session.PaymentIntentId);
                await _unitOfWork.CompleteAsync();

                Response.Headers.Append("Location", session.Url);
                return new StatusCodeResult(303);
            }
            catch (Exception)
            {
                // Fallback for local development
                await _unitOfWork.OrderHeaders.UpdateStatusAsync(orderHeader.Id, SD.StatusApproved, SD.PaymentStatusApproved);
                await _unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(OrderConfirmation), new { id = orderHeader.Id });
            }
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var orderHeader = await _unitOfWork.OrderHeaders.GetByIdAsync(id);
            if (orderHeader == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(orderHeader.SessionId))
            {
                try
                {
                    var service = new SessionService();
                    Session session = await service.GetAsync(orderHeader.SessionId);
                    if (session.PaymentStatus?.ToLower() == "paid")
                    {
                        await _unitOfWork.OrderHeaders.UpdateStripePaymentIdAsync(id, session.Id, session.PaymentIntentId);
                        await _unitOfWork.OrderHeaders.UpdateStatusAsync(id, SD.StatusApproved, SD.PaymentStatusApproved);
                        await _unitOfWork.CompleteAsync();
                    }
                }
                catch
                {
                    // Fallback for local development
                    await _unitOfWork.OrderHeaders.UpdateStatusAsync(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    await _unitOfWork.CompleteAsync();
                }
            }
            else
            {
                await _unitOfWork.OrderHeaders.UpdateStatusAsync(id, SD.StatusApproved, SD.PaymentStatusApproved);
                await _unitOfWork.CompleteAsync();
            }

            var userId = orderHeader.ApplicationUserId;
            var carts = (await _unitOfWork.ShoppingCarts.FindAsync(u => u.ApplicationUserId == userId)).ToList();
            foreach (var c in carts)
            {
                _unitOfWork.ShoppingCarts.Remove(c);
            }
            await _unitOfWork.CompleteAsync();

            HttpContext.Session.SetInt32(SD.SessionCart, 0);

            return View(orderHeader);
        }
    }
}
