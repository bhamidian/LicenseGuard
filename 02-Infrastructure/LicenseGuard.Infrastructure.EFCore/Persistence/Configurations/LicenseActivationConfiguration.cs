using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class LicenseActivationConfiguration : IEntityTypeConfiguration<LicenseActivation>
{
    public void Configure(EntityTypeBuilder<LicenseActivation> builder)
    {
        builder.Property(activation => activation.MachineId)
            .HasConversion(id => id.Value, value => MachineId.Create(value))
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(activation => activation.InstanceId)
            .HasConversion(id => id.Value, value => InstanceId.Create(value))
            .HasMaxLength(256)
            .IsRequired();
    }
}
