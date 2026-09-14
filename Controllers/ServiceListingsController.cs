using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CommUnityHub.Models;
using CommUnityHub.Repositories;

namespace CommUnityHub.Controllers
{
    public class ServiceListingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceListingsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int? categoryId, string? suburb, string? searchTerm)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            ViewBag.Categories = new SelectList(categories.Where(c => c.IsActive).OrderBy(c => c.Name), "Id", "Name", categoryId);
            ViewBag.SelectedCategory = categoryId;
            ViewBag.CurrentSuburb = suburb;
            ViewBag.SearchTerm = searchTerm;

            var listings = await _unitOfWork.ServiceListings.FilterListingsAsync(categoryId, suburb, searchTerm);
            return View(listings);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _unitOfWork.ServiceListings.GetListingWithCategoryByIdAsync(id.Value);
            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropDownList();
            var listing = new ServiceListing
            {
                ExchangeType = "Free Skill Swap",
                DepositRequired = false,
                EstimatedCost = 0.00m
            };
            return View(listing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceListing listing)
        {
            if (listing.DepositRequired && listing.EstimatedCost <= 0)
            {
                ModelState.AddModelError("EstimatedCost", "Please enter an estimated deposit or cost greater than $0 when deposit is required.");
            }

            if (ModelState.IsValid)
            {
                listing.CreatedAt = DateTime.Now;
                await _unitOfWork.ServiceListings.AddAsync(listing);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Service listing created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCategoriesDropDownList(listing.CategoryId);
            return View(listing);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _unitOfWork.ServiceListings.GetByIdAsync(id.Value);
            if (listing == null)
            {
                return NotFound();
            }

            await PopulateCategoriesDropDownList(listing.CategoryId);
            return View(listing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceListing listing)
        {
            if (id != listing.Id)
            {
                return NotFound();
            }

            if (listing.DepositRequired && listing.EstimatedCost <= 0)
            {
                ModelState.AddModelError("EstimatedCost", "Please enter an estimated deposit or cost greater than $0 when deposit is required.");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.ServiceListings.Update(listing);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Service listing updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateCategoriesDropDownList(listing.CategoryId);
            return View(listing);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _unitOfWork.ServiceListings.GetListingWithCategoryByIdAsync(id.Value);
            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var listing = await _unitOfWork.ServiceListings.GetByIdAsync(id);
            if (listing == null)
            {
                return NotFound();
            }

            _unitOfWork.ServiceListings.Remove(listing);
            await _unitOfWork.CompleteAsync();
            TempData["SuccessMessage"] = "Service listing deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesDropDownList(object? selectedCategory = null)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var activeCategories = categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
            ViewBag.CategoryId = new SelectList(activeCategories, "Id", "Name", selectedCategory);
        }
    }
}
