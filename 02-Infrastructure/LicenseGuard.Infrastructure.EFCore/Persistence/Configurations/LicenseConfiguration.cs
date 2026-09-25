using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.HasAlternateKey(license => new { license.Id, license.SubscriptionId });

        builder.Property(license => license.Key)
            .HasConversion(key => key.Value, value => LicenseKey.Create(value))
            .HasMaxLength(64)
            .IsRequired();
        builder.HasIndex(license => license.Key).IsUnique();

        builder.Property(license => license.Signature)
            .HasConversion(signature => signature.Value, value => LicenseSignature.Create(value))
            .HasMaxLength(2048)
            .IsRequired();

        builder.OwnsOne(license => license.AutoRenewal, owned =>
        {
            owned.Property(renewal => renewal.IsEnabled)
                .HasColumnName("AutoRenewalEnabled")
                .IsRequired();
            owned.Property(renewal => renewal.Plan)
                .HasColumnName("AutoRenewalPlan");
        });

        builder.HasMany(license => license.Activations)
            .WithOne(activation => activation.License)
            .HasForeignKey(activation => activation.LicenseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(license => license.StatusHistory)
            .WithOne(history => history.License)
            .HasForeignKey(history => history.LicenseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(license => license.LicenseFeatures)
            .WithOne(licenseFeature => licenseFeature.License)
            .HasForeignKey(licenseFeature => licenseFeature.LicenseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(license => license.Limits)
            .WithOne(limit => limit.License)
            .HasForeignKey(limit => limit.LicenseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(license => license.AuditLogs)
            .WithOne(log => log.License)
            .HasForeignKey(log => log.LicenseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
