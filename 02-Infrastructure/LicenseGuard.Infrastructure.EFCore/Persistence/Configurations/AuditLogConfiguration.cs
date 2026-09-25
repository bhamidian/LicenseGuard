using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("LicenseAuditLogs");
        builder.HasKey(log => log.Id);
        builder.HasIndex(log => log.LicenseId);
        builder.HasIndex(log => log.CreatedAt);
        builder.HasIndex(log => new { log.EntityType, log.EntityId });
    }
}
