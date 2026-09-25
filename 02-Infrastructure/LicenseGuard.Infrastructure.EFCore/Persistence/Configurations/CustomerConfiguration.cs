using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasMany(customer => customer.Subscriptions)
            .WithOne(subscription => subscription.Customer)
            .HasForeignKey(subscription => subscription.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
