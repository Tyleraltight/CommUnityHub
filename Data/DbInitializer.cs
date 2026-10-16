using CommUnityHub.Models;
using Microsoft.AspNetCore.Identity;

namespace CommUnityHub.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context, UserManager<ApplicationUser>? userManager = null, RoleManager<IdentityRole>? roleManager = null)
        {
            context.Database.EnsureCreated();

            // Seed Roles
            if (roleManager != null)
            {
                if (!roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                    roleManager.CreateAsync(new IdentityRole(SD.Role_Provider)).GetAwaiter().GetResult();
                    roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                }
            }

            // Seed Admin and Demo Users
            if (userManager != null)
            {
                var adminEmail = "admin@communityhub.nz";
                var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        Name = "Tyler Xu (Admin)",
                        PhoneNumber = "021-555-8888",
                        StreetAddress = "12 High Street",
                        City = "Lower Hutt",
                        Suburb = "Petone",
                        PostalCode = "5012",
                        EmailConfirmed = true
                    };
                    userManager.CreateAsync(adminUser, "Admin123!").GetAwaiter().GetResult();
                    userManager.AddToRoleAsync(adminUser, SD.Role_Admin).GetAwaiter().GetResult();
                }

                var residentEmail = "resident@communityhub.nz";
                var residentUser = userManager.FindByEmailAsync(residentEmail).GetAwaiter().GetResult();
                if (residentUser == null)
                {
                    residentUser = new ApplicationUser
                    {
                        UserName = residentEmail,
                        Email = residentEmail,
                        Name = "Sarah Jenkins",
                        PhoneNumber = "022-777-6666",
                        StreetAddress = "45 Queens Drive",
                        City = "Lower Hutt",
                        Suburb = "Lower Hutt",
                        PostalCode = "5010",
                        EmailConfirmed = true
                    };
                    userManager.CreateAsync(residentUser, "Resident123!").GetAwaiter().GetResult();
                    userManager.AddToRoleAsync(residentUser, SD.Role_Customer).GetAwaiter().GetResult();
                }
            }

            if (!context.ServiceCategories.Any())
            {
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

            // Seed Sample Orders for Order and Shipment Management demonstration
            if (!context.OrderHeaders.Any())
            {
                var paidListing = context.ServiceListings.FirstOrDefault(l => l.EstimatedCost > 0)
                                  ?? context.ServiceListings.FirstOrDefault();
                var listingId = paidListing != null ? paidListing.Id : 1;
                var unitPrice = paidListing != null && paidListing.EstimatedCost > 0 ? paidListing.EstimatedCost : 10.00m;

                var defaultUserId = "resident_demo_01";
                if (userManager != null)
                {
                    var residentUser = userManager.FindByEmailAsync("resident@communityhub.nz").GetAwaiter().GetResult();
                    if (residentUser != null) defaultUserId = residentUser.Id;
                }

                var sampleOrders = new OrderHeader[]
                {
                    new OrderHeader
                    {
                        ApplicationUserId = defaultUserId,
                        Name = "Sarah Jenkins",
                        PhoneNumber = "021-555-9012",
                        StreetAddress = "78 Jackson Street",
                        City = "Lower Hutt",
                        Suburb = "Petone",
                        PostalCode = "5012",
                        OrderDate = DateTime.Now.AddDays(-3),
                        ShippingDate = DateTime.Now.AddDays(-1),
                        OrderTotal = unitPrice * 2,
                        OrderStatus = SD.StatusShipped,
                        PaymentStatus = SD.PaymentStatusApproved,
                        Carrier = "NZ Post Local Courier",
                        TrackingNumber = "NZP-88234190-WN",
                        PaymentDate = DateTime.Now.AddDays(-3)
                    },
                    new OrderHeader
                    {
                        ApplicationUserId = defaultUserId,
                        Name = "Liam Gallagher",
                        PhoneNumber = "022-345-6789",
                        StreetAddress = "15 Victoria Street",
                        City = "Wellington",
                        Suburb = "Wellington Central",
                        PostalCode = "6011",
                        OrderDate = DateTime.Now.AddDays(-1),
                        OrderTotal = unitPrice,
                        OrderStatus = SD.StatusInProcess,
                        PaymentStatus = SD.PaymentStatusApproved,
                        Carrier = "CourierPost",
                        TrackingNumber = "CP-99014287-NZ",
                        PaymentDate = DateTime.Now.AddDays(-1)
                    },
                    new OrderHeader
                    {
                        ApplicationUserId = defaultUserId,
                        Name = "Michael Chang",
                        PhoneNumber = "027-654-3210",
                        StreetAddress = "34 Cuba Street",
                        City = "Lower Hutt",
                        Suburb = "Alicetown",
                        PostalCode = "5010",
                        OrderDate = DateTime.Now,
                        OrderTotal = unitPrice * 3,
                        OrderStatus = SD.StatusApproved,
                        PaymentStatus = SD.PaymentStatusApproved,
                        PaymentDate = DateTime.Now
                    }
                };

                foreach (var o in sampleOrders)
                {
                    context.OrderHeaders.Add(o);
                }
                context.SaveChanges();

                var sampleOrderDetails = new OrderDetail[]
                {
                    new OrderDetail
                    {
                        OrderHeaderId = sampleOrders[0].Id,
                        ServiceListingId = listingId,
                        Count = 2,
                        Price = unitPrice
                    },
                    new OrderDetail
                    {
                        OrderHeaderId = sampleOrders[1].Id,
                        ServiceListingId = listingId,
                        Count = 1,
                        Price = unitPrice
                    },
                    new OrderDetail
                    {
                        OrderHeaderId = sampleOrders[2].Id,
                        ServiceListingId = listingId,
                        Count = 3,
                        Price = unitPrice
                    }
                };

                foreach (var d in sampleOrderDetails)
                {
                    context.OrderDetails.Add(d);
                }
                context.SaveChanges();
            }
        }
    }
}
