using LicenseGuard.Domain.Entities;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Endpoint.WebApi.Identity;

public static class IdentitySeeder
{
    private static readonly (string UserName, string Email, bool IsAdmin)[] SeedAccounts =
    [
        ("seed.admin1", "admin1@example.test", true),
        ("seed.admin2", "admin2@example.test", true),
        ("seed.customer1", "customer1@example.test", false),
        ("seed.customer2", "customer2@example.test", false),
        ("seed.customer3", "customer3@example.test", false),
        ("seed.customer4", "customer4@example.test", false)
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "Customer");

        foreach (var account in SeedAccounts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await userManager.FindByNameAsync(account.UserName);
            if (user is null)
            {
                user = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = account.UserName,
                    Email = account.Email,
                    EmailConfirmed = false
                };

                var createResult = await userManager.CreateAsync(user);
                EnsureSucceeded(createResult, $"creating seeded account '{account.UserName}'");
            }
            else if (!string.Equals(user.Email, account.Email, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Seed account '{account.UserName}' exists with an unexpected email address.");
            }

            var roleName = account.IsAdmin ? "Admin" : "Customer";
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var roleResult = await userManager.AddToRoleAsync(user, roleName);
                EnsureSucceeded(roleResult, $"assigning role '{roleName}' to '{account.UserName}'");
            }

            var profileExists = await dbContext.UserProfiles
                .AnyAsync(profile => profile.AppUserId == user.Id, cancellationToken);
            if (!profileExists)
            {
                if (account.IsAdmin)
                    dbContext.Admins.Add(new Admin(user.Id));
                else
                    dbContext.Customers.Add(new Customer(user.Id));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole<Guid>> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        EnsureSucceeded(result, $"creating role '{roleName}'");
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
            throw new InvalidOperationException($"Identity seeding failed while {operation}: {errors}");
        }
    }
}
