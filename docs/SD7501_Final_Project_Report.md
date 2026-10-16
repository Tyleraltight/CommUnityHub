# SD7501 Web Application Development
## Final Project Technical Report

Student Name: Yusen Xu (Tyler)
Student ID: 22503513
Course: SD7501 Web Application Development
Programme: Bachelor of Information Technology
Institution: School of Innovation, Design and Technology, Whitireia and WelTec
Instructor: Anjali de Silva
Project Title: CommUnity Hub: Hyperlocal Community Service and Logistics Fulfillment Platform
Project Repository: https://github.com/Tyleraltight/CommUnityHub

1. Business, Technical, and Social Implications

1.1 Business Model and Economic Implications
CommUnity Hub operates as a hyperlocal service directory rather than a commercial gig marketplace. Mainstream gig apps take 15% to 30% commissions, inflating costs for minor household tasks and discouraging casual neighborly help. CommUnity Hub eliminates fees for basic trades. A resident can exchange two hours of programming tutoring for lawn mowing without cash leaving either pocket.

Direct barter breaks down when physical materials enter the job. A neighbor fixing a garden fence still needs screws, replacement timber, and fuel. CommUnity Hub solves this with a split transaction design. Skill swaps remain free. Physical supplies, tool hire deposits, and consumables route through an integrated payment pipeline. This model lowers costs for households across Lower Hutt and Petone.

Sustainability represents the primary business constraint. Commercial marketplaces spend revenues on marketing and dispute staff. CommUnity Hub relies on community self-governance and provider accountability. Without corporate insurance beyond refundable deposits, dispute handling depends on interpersonal communication. The platform functions as a neighborhood utility rather than a venture startup.

1.2 Technical Architecture and Infrastructure Implications
CommUnity Hub relies on ASP.NET Core 8 MVC and EF Core with SQL Server. ASP.NET Core delivers fast throughput and native dependency injection. The Kestrel server runs optimized C# code. Listing queries execute smoothly on local hardware.

Persistence uses the Repository and Unit of Work patterns. Keeping DbContext calls behind repositories isolates controllers from SQL operations. Object-relational mapping requires care. Joining listings, categories, and order lines can cause N+1 query overhead if loaded lazily. Eager loading with Include in ServiceListingRepository prevents duplicate queries.

Shopping cart state requires balancing server scalability with user convenience. HTTP is stateless. Managing carts requires session memory backed by browser cookies. In development, in-memory caching stores cart state. For Azure App Service deployments with multiple instances, Redis cache stores are needed to prevent sticky session dependencies.

1.3 Social Implications and Community Cohesion
The core mission centers on neighborhood connection. Modern suburbs often suffer from social fragmentation. CommUnity Hub acts as a noticeboard converting latent practical skills into social capital. A retired builder can repair a window latch in exchange for home internet troubleshooting. This exchange encourages intergenerational contact across Wellington and Lower Hutt.

Digital equity presents a social challenge. Older residents with trade experience may find multi-step checkouts difficult. Complex interfaces exclude people who have the most practical skills to offer. The UI uses responsive Bootstrap 5 cards, clean text, and simple forms that work on both phones and PCs.

Trust is vital for home visits. Commercial apps demand formal checks, while CommUnity Hub builds trust through transparent profiles, suburb boundaries, and user accounts stored in ASP.NET Core Identity.

1.4 Real-World Limitations and Operational Challenges
The primary real-world hurdle is the marketplace cold-start problem. A directory holds little value until local providers post listings. Providers will not post if searches yield no responses. Initial traction requires grassroots outreach through community centers and libraries.

Physical service verification remains challenging. Software cannot inspect whether a lawn was mowed or a pipe repaired. The platform tracks milestones from Pending to Shipped, leaving final completion to mutual agreement.

2. Application of ASP.NET Core Principles

2.1 Model-View-Controller Architectural Separation
ASP.NET Core MVC separates markup, processing, and domain data. The Model layer defines entities: ServiceListing, ServiceCategory, OrderHeader, OrderDetail, and ShoppingCart. DataAnnotations validate constraints at the data layer.

The View layer uses Razor templates. Views consume ViewModels such as ShoppingCartVM and OrderVM. Combining entities into ViewModels blocks over-posting risks and protects database tables from unwanted form edits.

Controllers coordinate HTTP traffic. They check ModelState, invoke repository methods through Unit of Work, and return views or redirects. CartController manages calculations without raw SQL. This keeps frontend and backend decoupled.

2.2 Dependency Injection and Service Lifetimes
ASP.NET Core provides an Inversion of Control container. CommUnity Hub sets service lifetimes in Program.cs. ApplicationDbContext registers as Scoped via AddDbContext. Each HTTP request gets an isolated context instance that disposes on completion.

IUnitOfWork registers as Scoped with AddScoped. Repositories in a request share one DbContext transaction. During checkout, CartController creates OrderHeader, writes OrderDetails, updates status flags, and calls CompleteAsync in one database commit.

Singleton services hold global configuration. StripeConfiguration stores API keys loaded at startup. HttpContextAccessor registers via AddHttpContextAccessor, letting layout views read session cart counts without memory leaks.

2.3 Middleware Execution Pipeline
The ASP.NET Core middleware pipeline processes incoming HTTP requests in an explicit sequence defined in Program.cs. Registration order dictates runtime execution.

In production environments, UseExceptionHandler catches unhandled exceptions and redirects visitors to a friendly error page. Raw database stack traces never leak to the public. UseHsts instructs browsers to enforce HTTPS transport security. UseStaticFiles serves CSS, script bundles, and images directly from wwwroot without invoking routing or authentication filters.

UseRouting matches URL paths against registered controller actions. Next, UseAuthentication reads identity cookies and populates the ClaimsPrincipal. UseAuthorization enforces role restrictions against the active user. UseSession executes after routing and authentication. This sequence lets session state associate with authenticated cookies and inject cart session IDs into response headers. Finally, MapControllerRoute and MapControllers dispatch execution to target endpoints.

2.4 Routing Strategies and Endpoint Architecture
CommUnity Hub blends two routing strategies to serve server-rendered views alongside RESTful consumers. Conventional routing powers standard MVC controllers through the default route pattern. URL segments map controller, action, and optional id directly to C# methods. Paths like /ServiceListings/Details/3 resolve without messy querystrings.

Attribute routing handles Stripe payment processing and order status lookups. PaymentApiController uses the ApiController and Route attributes, exposing explicit REST endpoints. Endpoints including POST api/payment/create-checkout-session and GET api/payment/order-status/{id} consume and return JSON payloads. The ApiController attribute automates model state validation. It returns 400 Bad Request responses when JSON payloads fail schema rules.

3. Final Improvements and Advanced Features

3.1 Order and Shipment Management
The third functional transaction entity combines OrderHeader and OrderDetail. This turns CommUnity Hub into an active fulfillment platform. OrderHeader tracks buyer identity, dates, totals, carrier names, and tracking numbers. OrderDetail stores item history and prices.

OrderController provides an operational dashboard. Providers track orders across five states: Pending, Approved, Processing, Shipped, and Cancelled. When tasks require tools or supplies, providers enter the carrier (NZ Post) and tracking number, advancing status to Shipped. CancelOrder handles cancellations and refunds.

3.2 Secure Shopping Cart State Management
The cart uses session memory and database records. ShoppingCart entities store user keys, listing references, and quantities. CartController handles AddToCart, Plus, Minus, and Remove actions.

Session state holds the item count under SessionCart. Each cart update recalculates totals. The layout reads this value to show a live badge. Session cookies use HttpOnly to block script theft and IsEssential to comply with privacy rules.

3.3 Stripe Payment Integration via RESTful API
Payment integration connects the app to Stripe. Skill trades carry no labor fee, but tool deposits and materials require payment. PaymentApiController offers a RESTful endpoint: POST api/payment/create-checkout-session. It validates order totals and creates a Stripe Checkout Session in NZD cents.

The API returns the Stripe session ID and checkout URL. Users redirect to Stripe to pay. After payment, Stripe returns users to OrderConfirmation. The controller verifies status, sets OrderHeader to Approved, logs PaymentIntentId, and clears the cart.

3.4 Identity and Role-Based Access Control
Security uses ASP.NET Core Identity. ApplicationUser extends IdentityUser with Name, StreetAddress, City, Suburb, and PostalCode. DbInitializer seeds three roles (Admin, Provider, Customer) and default accounts.

Role checks protect key actions. Browsing is public, while posting services and dispatching orders require authentication. Passwords use PBKDF2 hashing.

4. Conceptual Framework Update and Project Evolution

4.1 Evolution from Assignment 1 and Phase 1
The system evolved across three milestones. Assignment 1 defined the architecture and relational design. Assignment 2 built the core, adding ServiceCategory and ServiceListing CRUD, combined search filters, and dual-layer validation.

Phase 1 lacked commerce features. Users could read posts, but could not book services or buy supplies. The Final Phase added OrderHeader, OrderDetail, ShoppingCart, and Stripe APIs, turning the directory into a functional exchange.

4.2 End-to-End Data Flow
The refined architecture supports an end-to-end user path. A resident searches listings by category and suburb, selecting a gardening service with a material deposit. Clicking Add to Cart sends a request to CartController, persisting a ShoppingCart record and updating the navigation badge. 

Proceeding to checkout opens the Summary view, capturing delivery address and contact fields. Submitting the form writes an OrderHeader record in Pending status and inserts OrderDetail line items. CartController calls Stripe to generate a hosted payment session and issues an HTTP 303 redirect. After payment approval, Stripe returns the user to OrderConfirmation, which updates the database to Approved and empties the cart. Administrators then review the order in OrderController, advancing it through Processing to Shipped.

5. Development Challenges and Engineering Strategies

5.1 Challenge 1: Harmonizing Guest Sessions with Authenticated Carts
Managing cart items before user login was a key challenge. Requiring login before adding items causes drop-offs. Relying only on user database IDs leads to null reference errors for guests.

CartController uses a dual-mode strategy. For logged-in users, it takes the NameIdentifier claim. For guests, it checks HttpContext.Session for a GuestUserId, creating one if absent. At checkout, customer fields link OrderHeader to the guest or user ID. This keeps browsing smooth.

5.2 Challenge 2: Synchronizing External Stripe Sessions with Relational State
Syncing external Stripe sessions with internal state was challenging. If a user pays on Stripe but closes the tab before returning, database records could stay Pending. Passing database IDs in query strings also poses security risks.

The system uses two-phase synchronization. At checkout creation, the controller saves Stripe SessionId to OrderHeader. In OrderConfirmation, it queries Stripe via SessionService.GetAsync to verify payment before setting Approved. PaymentApiController also provides a status endpoint for verification.

5.3 Challenge 3: Enforcing Strict Referential Integrity Across Cascades
Deleting records with linked foreign keys caused database exceptions. If a provider deletes a listing tied to past orders, cascade deletes would erase purchase history, while restrict rules throw errors.

The solution uses checks on both tiers. Foreign keys set DeleteBehavior.Restrict. ServiceListingsController checks UnitOfWork before deletion. If an OrderDetail references the listing, deletion is blocked with a notice to mark it unavailable instead.

6. Security Enhancements and Future Scalability

6.1 Implemented Security Defenses
Security controls in CommUnity Hub operate across multiple application layers. Transport communications enforce HTTPS with HSTS headers. Model binding uses strongly typed ViewModels to block over-posting attacks that attempt to modify restricted role fields.

Razor forms include Html.AntiForgeryToken tokens, validated by ValidateAntiForgeryToken attributes on POST controller actions to block Cross-Site Request Forgery. Entity Framework Core executes parameterized queries. This stops SQL injection attacks. Client validation runs via jQuery Unobtrusive, while server-side ModelState checks guarantee that disabling JavaScript cannot bypass database rules.

6.2 Future Enhancements for Security and Scalability
Future upgrades include moving Stripe keys and database credentials to Azure Key Vault with managed identities, removing secrets from files.

Session state should move to Azure Cache for Redis for multi-instance scaling. Rate-limiting middleware on PaymentApiController will protect payment routes from brute-force traffic.

7. Summary, Lessons Learned, and Recommendations for Future Students

7.1 Summary of Project Outcomes
The CommUnity Hub final project satisfies all curriculum goals established for SD7501. Building upon Assignment 1 and 2, the application delivers a complete community service platform. Key implementations include a third functional transaction entity handling order workflows and shipment tracking, a secure session shopping cart with real-time UI badge synchronization, a RESTful API integrating Stripe Checkout, and deployment readiness for Azure App Service.

xUnit tests verified core seams: cart calculation, order workflow transitions, repository writes, and payment endpoints. Following MVC, DI, and Repository patterns produced an organized codebase.

7.2 Practical Recommendations for Future Students
Building a transactional web application in ASP.NET Core offers valuable engineering lessons:

- Plan Relational Constraints Early: Map out foreign key behaviors and navigation collections before writing controller logic. Relying on default cascade deletes leads to data loss, while unhandled restrict rules cause runtime crashes. Implementing repository pre-checks avoids costly refactoring later.
- Abstract External APIs: Third-party services like Stripe update SDKs regularly and need mock setups during automated test runs. Isolating payment calls behind REST endpoints or service wrappers ensures unit tests run fast in offline settings.
- Treat ViewModels as Security Boundaries: Never pass raw Entity Framework domain models directly to Razor views or accept them uninspected in POST methods. ViewModels protect against over-posting attacks and prevent circular reference errors when serializing JSON data.

8. Project Resources and Academic References

8.1 GitHub Repository
The complete source code, test project, and commit history are hosted on GitHub:
Repository Link: https://github.com/Tyleraltight/CommUnityHub

8.2 Academic and Professional References
Freeman, A. (2022). Pro ASP.NET Core 6: Develop Cloud-Ready Web Applications Using MVC, Blazor, and Razor Pages (9th ed.). Apress. https://doi.org/10.1007/978-1-4842-7957-1
Martin, R. C. (2021). Clean Craftsmanship: Disciplines, Standards, and Ethics. Addison-Wesley Professional.
Microsoft Corporation. (2024). ASP.NET Core Architecture and Application Models. Microsoft Learn. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/
Microsoft Corporation. (2024). Overview of Entity Framework Core. Microsoft Learn. https://learn.microsoft.com/en-us/ef/core/
Nixon, R. (2021). Learning PHP, MySQL & JavaScript: With jQuery, CSS & HTML5 (6th ed.). O'Reilly Media.
Stripe Corporation. (2024). Stripe API Reference and Integration Guidelines. Stripe Documentation. https://stripe.com/docs/api
Troelsen, A., & Japikse, P. (2022). Pro C# 10 with .NET 6: Foundational Principles and Practices in Programming (11th ed.). Apress. https://doi.org/10.1007/978-1-4842-7869-7
