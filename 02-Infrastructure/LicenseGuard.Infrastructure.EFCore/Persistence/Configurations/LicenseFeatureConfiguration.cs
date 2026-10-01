using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class LicenseFeatureConfiguration : IEntityTypeConfiguration<LicenseFeature>
{
    public void Configure(EntityTypeBuilder<LicenseFeature> builder)
    {
        builder.Property(link => link.FeatureCodeSnapshot).HasMaxLength(100).IsRequired();
        builder.HasIndex(link => new { link.LicenseId, link.FeatureId }).IsUnique();
    }
}
