using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class ProductFeatureConfiguration : IEntityTypeConfiguration<ProductFeature>
{
    public void Configure(EntityTypeBuilder<ProductFeature> builder)
    {
        builder.HasIndex(link => new { link.ProductId, link.FeatureId }).IsUnique();
    }
}
