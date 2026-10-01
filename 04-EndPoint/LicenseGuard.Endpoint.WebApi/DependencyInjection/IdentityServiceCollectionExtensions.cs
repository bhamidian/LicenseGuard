using LicenseGuard.Domain.Entities;
using LicenseGuard.Endpoint.WebApi.Configuration;
using LicenseGuard.Endpoint.WebApi.Identity;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddLicenseGuardIdentity(this IServiceCollection services)
    {
        services.AddDataProtection();
        services.AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.AddSingleton<IConfigureOptions<IdentityOptions>, IdentityPolicyConfiguration>();
        return services;
    }
}
