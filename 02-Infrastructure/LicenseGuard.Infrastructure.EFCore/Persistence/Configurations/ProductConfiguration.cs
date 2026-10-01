using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(product => product.Code).IsUnique();

        builder.HasMany(product => product.Plans)
            .WithOne(plan => plan.Product)
            .HasForeignKey(plan => plan.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(product => product.ProductFeatures)
            .WithOne(productFeature => productFeature.Product)
            .HasForeignKey(productFeature => productFeature.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
