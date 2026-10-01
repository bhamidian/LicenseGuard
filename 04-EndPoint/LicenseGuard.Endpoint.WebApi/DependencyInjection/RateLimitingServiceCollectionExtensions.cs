using LicenseGuard.Endpoint.WebApi.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddLicenseGuardRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(_ => { });
        services.AddSingleton<IdentityRateLimitConfiguration>();
        services.AddSingleton<IConfigureOptions<RateLimiterOptions>>(provider =>
            provider.GetRequiredService<IdentityRateLimitConfiguration>());
        services.AddSingleton<IConfigureOptions<MvcOptions>>(provider =>
            provider.GetRequiredService<IdentityRateLimitConfiguration>());
        return services;
    }
}
