using Microsoft.EntityFrameworkCore;
using CommUnityHub.Models;

namespace CommUnityHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ServiceCategory> ServiceCategories { get; set; } = null!;
        public DbSet<ServiceListing> ServiceListings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceListing>()
                .HasOne(l => l.Category)
                .WithMany(c => c.ServiceListings)
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
