# SD7501 Web Application Development
## Assignment 2 Project Report (Phase 1)
**Project Title:** CommUnity Hub: Hyperlocal Skill Sharing and Community Service Platform  
**Course:** SD7501 Web Application Development  
**Institution:** School of Innovation, Design and Technology, Whitireia and WelTec  
**Student Name:** Yusen Xu (Tyler)  
**Student ID:** 22503513  
**Instructor:** Anjali de Silva  

---

## 1. Application Description and Requirements Specification

### 1.1 Application Overview and Purpose
CommUnity Hub is an ASP.NET Core MVC web application built with Entity Framework Core and SQL Server LocalDB. It provides a local directory where neighborhood residents can exchange practical skills and services (such as lawn care, plumbing repairs, home tech setup, or language tutoring).

Mainstream gig-economy apps typically take steep service commissions and cater to commercial contractors rather than immediate neighbors. This project focuses on non-commercial and low-cost community exchanges, letting members list services, find help within their own suburb, and negotiate either direct skill swaps or basic cost reimbursements without platform fees.

### 1.2 Key Features Implemented in Phase 1
* Category administration: Full CRUD operations for service categories so tasks remain neatly organized.
* Service listings: Users can create, view, update, and remove listings tied directly to categories via foreign keys.
* Combined search filters: Users can query listings simultaneously by category dropdown, suburb text, and general keywords.
* Two-tier input validation: Client-side jQuery checks prevent empty or invalid submissions before they reach the server, while ASP.NET Core model validation enforces constraints on the backend.
* Responsive interface: Built on Bootstrap 5 cards, breadcrumb navigation, and alert banners that adapt to mobile and desktop screens.

### 1.3 Project Deliverables
* Visual Studio solution containing the full ASP.NET Core MVC web app.
* Database migration scripts and initial seed data for SQL Server LocalDB.
* Data access layer configured with the Repository and Unit of Work patterns.
* Technical project documentation covering architecture, class designs, implementation hurdles, and next-phase milestones.

### 1.4 Functional Requirements
* FR1 (Category Management): Administrators can create, view, edit, and remove service categories with validation applied to names, descriptions, and display orders.
* FR2 (Service Posting): Users can post service listings containing a title, detailed description, contact email and phone, suburb, and preferred exchange type.
* FR3 (Relational Integrity): The application prevents deleting any category that still has active listings attached to it.
* FR4 (Search and Filtering): The listing index supports filtering by category selection, suburb match, and text keywords.
* FR5 (Conditional Cost Rules): If a provider checks the deposit or reimbursement requirement, the form must enforce a positive numerical dollar amount.

### 1.5 Non-Functional Requirements
* NFR1 (Performance): Filtered listing queries should complete under 200 ms on a local development machine using SQL Server LocalDB.
* NFR2 (Reliability): The web app should handle invalid inputs and unexpected routes gracefully without exposing raw developer stack traces.
* NFR3 (Maintainability): Data access logic remains separate from controllers through repositories and a unit of work, simplifying future unit testing.
* NFR4 (Usability): Layouts and forms follow Bootstrap 5 responsive guidelines to function cleanly on both phone screens and desktop browsers.

---

## 2. Conceptual Framework and Evolution Roadmap

### 2.1 Conceptual Architecture
CommUnity Hub follows a standard multi-tier design in ASP.NET Core MVC:
* Presentation tier (Views and ViewModels): Razor templates with Bootstrap 5 styles, managing HTML generation, form inputs, and client-side jQuery checks.
* Controller tier: Receives HTTP requests, runs model binding, inspects `ModelState`, and hands execution over to the Unit of Work.
* Data access tier (Repositories and Unit of Work): Exposes specific query methods (such as category inclusion and multi-field filters) while keeping EF Core LINQ statements out of controllers.
* Persistence tier (Entity Framework Core): Manages schema mapping and queries against SQL Server LocalDB via `ApplicationDbContext`.

### 2.2 Phase 1 to Final Project Evolution
Phase 1 puts down the database models, repository abstractions, and baseline CRUD operations. Looking ahead to the final submission, the system will expand in three practical areas:

1. User authentication and roles:
   * Adding ASP.NET Core Identity for secure logins.
   * Setting up distinct roles (Administrators, Providers, and Residents) so only admins can alter category definitions.

2. Booking and exchange workflows:
   * Creating a `ServiceBooking` entity that connects a resident to a listing.
   * Tracking the request state across stages (Submitted, Accepted, In Progress, Completed, and Cancelled).

3. Payment integration model:
   * While basic skill swaps carry no hourly wages, practical community jobs still need payment handling in two scenarios:
     * Commitment deposits: Small escrow amounts held when a booking is confirmed to discourage no-shows, returned once both users mark the job finished.
     * Material costs: Direct reimbursement for supplies (like replacement plumbing parts or garden fuel) processed through Stripe Checkout.
   * The Phase 1 model already includes `ExchangeType`, `DepositRequired`, and `EstimatedCost` to support these additions without requiring schema rewrites later.

### 2.3 Development Milestones
* Milestone 1 (Assignment 2, Weeks 1-4): Complete the schema, EF Core migrations, repository pattern, CRUD views, dual validation, and initial documentation.
* Milestone 2 (Final Project, Weeks 5-8): Wire up ASP.NET Core Identity, account registration, role authorization, and listing ownership.
* Milestone 3 (Final Project, Weeks 9-11): Build the booking workflow engine, Stripe deposit handling, and email status updates.
* Milestone 4 (Final Project, Weeks 12-14): Add review and rating mechanisms, run full integration tests, and finalize project deliverables.

---

## 3. Technical Implementation Details

### 3.1 Domain Models
The `CommUnityHub.Models` namespace contains two primary entities:
* `ServiceCategory`: Stores basic category info (`Id`, `Name`, `Description`, `IconClass`, `DisplayOrder`, `IsActive`) and references related records through the `ServiceListings` navigation collection.
* `ServiceListing`: Captures the listing specifics (`Id`, `CategoryId`, `Title`, `Description`, `ProviderName`, `ContactEmail`, `ContactPhone`, `Suburb`, `ExchangeType`, `DepositRequired`, `EstimatedCost`, `IsAvailable`, `CreatedAt`) alongside a navigation reference back to `Category`.

### 3.2 Data Context Configuration
`ApplicationDbContext` extends EF Core's `DbContext` and defines the `DbSet` collections. Inside `OnModelCreating`, the foreign key mapping enforces strict deletion rules:
```csharp
modelBuilder.Entity<ServiceListing>()
    .HasOne(l => l.Category)
    .WithMany(c => c.ServiceListings)
    .HasForeignKey(l => l.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);
```
Using `DeleteBehavior.Restrict` tells SQL Server not to cascade deletes automatically when a category is deleted, keeping child listings intact and forcing application-level checks.

### 3.3 Repository Pattern and Unit of Work Implementation
Data operations are organized across generic and domain-specific repository classes:
* `IRepository<T>` and `Repository<T>`: Handle standard database CRUD operations like `GetAllAsync`, `GetByIdAsync`, `FindAsync`, `AddAsync`, `Update`, and `Remove`.
* `IServiceListingRepository` and `ServiceListingRepository`: Extend the generic base with customized query methods. `GetListingsWithCategoryAsync()` runs eager loading using `.Include()`, while `FilterListingsAsync()` dynamically constructs LINQ predicates for category, suburb, and text inputs.
* `IUnitOfWork` and `UnitOfWork`: Group the repositories under a single `ApplicationDbContext` instance and expose `CompleteAsync()` so updates save as a single unit.

### 3.4 Controller Logic and Model Binding
* `CategoriesController`: Handles administrative category views. Its `DeleteConfirmed` action checks the unit of work before removing a category, redirecting with a `TempData` alert if any listings remain attached.
* `ServiceListingsController`: Manages the public search and listing entry forms. Actions use ASP.NET Core model binding to parse form posts, while category select elements are populated using `SelectList` helpers fed by the repository.
* `HomeController`: Acts as the landing page, showing basic stats like total available listings, category tallies, and the newest four service entries.

### 3.5 Dual-Validation Strategy
Validation happens at two separate application boundaries:
* Server-side checks: Domain model classes use standard DataAnnotations (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Phone]`, `[Range]`). In controller POST actions, `ModelState.IsValid` guards all repository calls. An additional backend rule verifies that whenever a user checks `DepositRequired`, `EstimatedCost` must exceed zero.
* Client-side checks: Razor views pull in `_ValidationScriptsPartial` so jQuery unobtrusive validation runs instantly in the browser. A custom script watches `#depositCheckbox`, showing or hiding the cost box and disabling or enabling the text field accordingly.

---

## 4. Development Challenges, Constraints, and Critical Reflection

### 4.1 Challenge 1: Cascading Deletes and Referential Integrity
When testing category deletions, SQL Server threw unhandled foreign key exceptions whenever a category still had listings referencing its primary key. Allowing cascade deletes was out of the question, as that would wipe out legitimate community posts without warning.

I resolved this on two levels. First, inside `ApplicationDbContext`, I set `DeleteBehavior.Restrict` in the Fluent API configuration to block automatic deletions at the database layer. Second, to prevent users from encountering raw database errors, I added a pre-check inside `CategoriesController.DeleteConfirmed`. Before executing the deletion, the controller calls `_unitOfWork.ServiceListings` to count associated records. If any exist, the action stops immediately, redirects back to the index view, and uses `TempData` to display a clear notice advising the user to clear or reassign those listings first.

### 4.2 Challenge 2: Eager Loading within the Repository Boundary
Listing cards in the UI need category names and badge icons, meaning the view relies on related parent data. The quick solution would have been calling `.Include()` straight inside the controller actions, but doing so leaks EF Core querying mechanics into the presentation layer and bypasses the repository abstraction.

To keep concerns separated, I added `IServiceListingRepository` on top of the generic base. Methods like `GetListingsWithCategoryAsync` and `FilterListingsAsync` live inside this interface, keeping the `.Include(l => l.Category)` calls tucked away inside `ServiceListingRepository`. The controller simply asks for data and receives ready-to-render collections without knowing how EF Core structured the join.

### 4.3 Challenge 3: Coordinated Dynamic Validation for Conditional Fields
The assignment rubric allocates 10 marks to server and client validation, specifically testing conditional inputs. In CommUnity Hub, deposits and cost reimbursements are optional, but once a user checks the deposit box, providing a realistic dollar value becomes mandatory.

Keeping the user interface responsive while protecting database integrity required changes on both ends. In the browser, a jQuery snippet hooks into `#depositCheckbox` change events, unhiding the cost input and removing its readonly attribute when checked. On the backend, client scripts cannot be trusted alone. In `ServiceListingsController`, a conditional guard checks if `DepositRequired` is true while `EstimatedCost` remains zero or negative. When triggered, it attaches a custom validation error to `ModelState`, blocking invalid submissions even if someone submits the form with JavaScript disabled.

### 4.4 Critical Reflection on Architecture Choices
Setting up the Unit of Work and Repository structure introduced considerable initial overhead compared to standard EF Core controller scaffolding. Creating interfaces, implementing wrapper classes, and wiring everything into dependency injection in `Program.cs` felt heavy for an early-stage app with only two tables.

That initial friction paid off once I started building the multi-parameter filter. Tweaking the LINQ search clauses in `ServiceListingRepository` required no adjustments to controller signatures or Razor views. The separation kept the controller methods small and focused on HTTP flow rather than query building. While it took longer to get the initial CRUD pages running, the codebase is much easier to reason about as we head into Phase 2 to add Identity and Stripe payments.

---

## 5. Project Resources and Academic References

### 5.1 Project Repository Link
* GitHub Repository: `https://github.com/Tyleraltight/CommUnityHub`
* Note: All commits, branch histories, and source code files are hosted in this public repository.

### 5.2 Academic and Professional References
* Freeman, A. (2022). *Pro ASP.NET Core 6: Develop Cloud-Ready Web Applications Using MVC, Blazor, and Razor Pages* (9th ed.). Apress.
* Martin, R. C. (2018). *Clean Architecture: A Craftsman's Guide to Software Structure and Design*. Prentice Hall.
* Microsoft Corporation. (2023). *Overview of Entity Framework Core*. Microsoft Learn. https://learn.microsoft.com/en-us/ef/core/
* Microsoft Corporation. (2023). *Model-View-Controller (MVC) pattern in ASP.NET Core*. Microsoft Learn. https://learn.microsoft.com/en-us/aspnet/core/mvc/overview
* Fowler, M. (2002). *Patterns of Enterprise Application Architecture*. Addison-Wesley Professional.
