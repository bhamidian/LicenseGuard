using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class LicenseFeatureConfiguration : IEntityTypeConfiguration<LicenseFeature>
{
    public void Configure(EntityTypeBuilder<LicenseFeature> builder)
    {
        builder.HasIndex(link => new { link.LicenseId, link.FeatureId }).IsUnique();
    }
}
