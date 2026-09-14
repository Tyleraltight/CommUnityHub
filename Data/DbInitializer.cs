using CommUnityHub.Models;

namespace CommUnityHub.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.ServiceCategories.Any())
            {
                return;
            }

            var categories = new ServiceCategory[]
            {
                new ServiceCategory { Name = "Home Maintenance", Description = "Repairs, painting, carpentry, and handyman tasks", IconClass = "bi-tools", DisplayOrder = 1, IsActive = true },
                new ServiceCategory { Name = "Tech Support & Tutoring", Description = "Computer troubleshooting, phone setup, software help, and programming lessons", IconClass = "bi-laptop", DisplayOrder = 2, IsActive = true },
                new ServiceCategory { Name = "Gardening & Outdoor", Description = "Lawn mowing, hedge trimming, weeding, and garden design", IconClass = "bi-tree", DisplayOrder = 3, IsActive = true },
                new ServiceCategory { Name = "Pet Care", Description = "Dog walking, pet sitting, feeding, and grooming help", IconClass = "bi-heart", DisplayOrder = 4, IsActive = true },
                new ServiceCategory { Name = "Language & Music", Description = "Conversational language practice, piano, guitar, and vocal lessons", IconClass = "bi-music-note", DisplayOrder = 5, IsActive = true }
            };

            foreach (var c in categories)
            {
                context.ServiceCategories.Add(c);
            }
            context.SaveChanges();

            var listings = new ServiceListing[]
            {
                new ServiceListing
                {
                    CategoryId = categories[0].Id,
                    Title = "Interior Wall Painting & Patching",
                    Description = "Experienced DIY painter offering room painting and plaster patching in exchange for help with web design or yard work.",
                    ProviderName = "David Miller",
                    ContactEmail = "david.miller@example.com",
                    ContactPhone = "021-555-1234",
                    Suburb = "Petone",
                    ExchangeType = "Free Skill Swap",
                    DepositRequired = false,
                    EstimatedCost = 0.00m,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new ServiceListing
                {
                    CategoryId = categories[1].Id,
                    Title = "Python & C# Basic Programming Tutoring",
                    Description = "IT undergraduate willing to tutor beginner Python or C# programming in exchange for conversational Spanish or guitar practice.",
                    ProviderName = "Alex Chen",
                    ContactEmail = "alex.chen@example.com",
                    ContactPhone = "022-789-4561",
                    Suburb = "Lower Hutt",
                    ExchangeType = "Free Skill Swap",
                    DepositRequired = false,
                    EstimatedCost = 0.00m,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now.AddDays(-3)
                },
                new ServiceListing
                {
                    CategoryId = categories[2].Id,
                    Title = "Lawn Mowing & Green Waste Removal",
                    Description = "I have a petrol lawnmower and line trimmer. Happy to mow your lawn in exchange for small appliance repair. Petrol cost $10.",
                    ProviderName = "James Wilson",
                    ContactEmail = "james.w@example.com",
                    ContactPhone = "027-321-9876",
                    Suburb = "Wellington Central",
                    ExchangeType = "Material Cost Covered",
                    DepositRequired = true,
                    EstimatedCost = 10.00m,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new ServiceListing
                {
                    CategoryId = categories[3].Id,
                    Title = "Weekend Dog Walking & Feeding",
                    Description = "Passionate dog lover offering weekend dog walking around local parks. Looking for math tutoring for high school sister.",
                    ProviderName = "Emma Watson",
                    ContactEmail = "emma.watson@example.com",
                    ContactPhone = "021-998-3344",
                    Suburb = "Petone",
                    ExchangeType = "Free Skill Swap",
                    DepositRequired = false,
                    EstimatedCost = 0.00m,
                    IsAvailable = true,
                    CreatedAt = DateTime.Now.AddDays(-1)
                }
            };

            foreach (var l in listings)
            {
                context.ServiceListings.Add(l);
            }
            context.SaveChanges();
        }
    }
}
