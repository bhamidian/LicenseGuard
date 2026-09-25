using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.HasMany(plan => plan.Subscriptions)
            .WithOne(subscription => subscription.Plan)
            .HasForeignKey(subscription => subscription.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(plan => plan.PlanFeatures)
            .WithOne(planFeature => planFeature.Plan)
            .HasForeignKey(planFeature => planFeature.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
