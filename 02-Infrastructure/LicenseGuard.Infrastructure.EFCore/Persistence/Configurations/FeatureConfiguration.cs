using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.HasIndex(feature => feature.Code).IsUnique();

        builder.HasMany(feature => feature.ProductFeatures)
            .WithOne(productFeature => productFeature.Feature)
            .HasForeignKey(productFeature => productFeature.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(feature => feature.PlanFeatures)
            .WithOne(planFeature => planFeature.Feature)
            .HasForeignKey(planFeature => planFeature.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(feature => feature.LicenseFeatures)
            .WithOne(licenseFeature => licenseFeature.Feature)
            .HasForeignKey(licenseFeature => licenseFeature.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
