using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfAppUserRepository(
    ApplicationDbContext dbContext,
    UserManager<AppUser> userManager) : IAppUserRepository
{
    public Task<AppUser?> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.PhoneNumber == phoneNumber, cancellationToken);

    public async Task<AppUser?> CreatePendingAsync(
        CreatePendingAppUserRecord record,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = record.UserName,
            Email = record.Email,
            PhoneNumber = record.PhoneNumber,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            LockoutEnabled = true
        };

        var result = await userManager.CreateAsync(user);
        return result.Succeeded ? user : null;
    }
}
