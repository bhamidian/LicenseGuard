using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        // Prevent two concurrent issuance requests from both replacing an empty current-license slot.
        builder.Property(subscription => subscription.CurrentLicenseId).IsConcurrencyToken();

        builder.HasOne(subscription => subscription.Product)
            .WithMany()
            .HasForeignKey(subscription => subscription.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(subscription => subscription.Licenses)
            .WithOne(license => license.Subscription)
            .HasForeignKey(license => license.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(subscription => subscription.CurrentLicense)
            .WithOne()
            .HasPrincipalKey<License>(license => new { license.Id, license.SubscriptionId })
            .HasForeignKey<Subscription>(subscription => new { subscription.CurrentLicenseId, subscription.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(subscription => subscription.Renewals)
            .WithOne()
            .HasForeignKey(renewal => renewal.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
