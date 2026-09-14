using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CommUnityHub.Models;
using CommUnityHub.Repositories;

namespace CommUnityHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var listings = await _unitOfWork.ServiceListings.GetListingsWithCategoryAsync();

            var viewModel = new HomeViewModel
            {
                FeaturedCategories = categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).Take(6).ToList(),
                RecentListings = listings.Where(l => l.IsAvailable).Take(4).ToList(),
                TotalCategoriesCount = categories.Count(c => c.IsActive),
                TotalListingsCount = listings.Count()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
