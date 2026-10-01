using LicenseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LicenseGuard.Infrastructure.EFCore.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasDiscriminator<string>("ProfileType")
            .HasValue<Customer>("Customer")
            .HasValue<Admin>("Admin");

        builder.HasOne<AppUser>()
            .WithOne()
            .HasForeignKey<User>(user => user.AppUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
