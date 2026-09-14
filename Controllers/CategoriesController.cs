using Microsoft.AspNetCore.Mvc;
using CommUnityHub.Models;
using CommUnityHub.Repositories;

namespace CommUnityHub.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var orderedCategories = categories.OrderBy(c => c.DisplayOrder).ToList();
            return View(orderedCategories);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            var listings = await _unitOfWork.ServiceListings.FindAsync(l => l.CategoryId == id.Value);
            category.ServiceListings = listings.OrderByDescending(l => l.CreatedAt).ToList();

            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCategory category)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceCategory category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Categories.Update(category);
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            var relatedListings = await _unitOfWork.ServiceListings.FindAsync(l => l.CategoryId == id.Value);
            ViewBag.ListingCount = relatedListings.Count();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var relatedListings = await _unitOfWork.ServiceListings.FindAsync(l => l.CategoryId == id);
            if (relatedListings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete category with associated service listings. Please reassign or remove listings first.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.CompleteAsync();
            TempData["SuccessMessage"] = "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
