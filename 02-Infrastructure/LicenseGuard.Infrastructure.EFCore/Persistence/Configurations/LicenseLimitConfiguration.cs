using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class LicenseLimitConfiguration : IEntityTypeConfiguration<LicenseLimit>
{
    public void Configure(EntityTypeBuilder<LicenseLimit> builder)
    {
        builder.Property(limit => limit.Code).HasMaxLength(100).IsRequired();
        builder.Property(limit => limit.Unit).HasMaxLength(32);
        builder.HasIndex(limit => new { limit.LicenseId, limit.Code }).IsUnique();
    }
}
