using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class LicenseStatusHistoryConfiguration : IEntityTypeConfiguration<LicenseStatusHistory>
{
    public void Configure(EntityTypeBuilder<LicenseStatusHistory> builder)
    {
        builder.HasKey(history => history.Id);
    }
}
