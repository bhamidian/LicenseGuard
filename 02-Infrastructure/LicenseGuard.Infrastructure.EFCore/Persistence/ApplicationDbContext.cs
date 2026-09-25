using LicenseGuard.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Infrastructure.EFCore.Persistence
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Feature> Features => Set<Feature>();
        public DbSet<ProductFeature> ProductFeatures => Set<ProductFeature>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<SubscriptionRenewal> SubscriptionRenewals => Set<SubscriptionRenewal>();
        public DbSet<License> Licenses => Set<License>();
        public DbSet<LicenseFeature> LicenseFeatures => Set<LicenseFeature>();
        public DbSet<LicenseLimit> LicenseLimits => Set<LicenseLimit>();
        public DbSet<LicenseActivation> LicenseActivations => Set<LicenseActivation>();
        public DbSet<LicenseStatusHistory> LicenseStatusHistory => Set<LicenseStatusHistory>();
        public DbSet<AuditLog> LicenseAuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
