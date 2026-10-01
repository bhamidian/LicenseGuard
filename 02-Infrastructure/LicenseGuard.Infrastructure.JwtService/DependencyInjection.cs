using LicenseGuard.Infrastructure.JwtService.Contracts;
using LicenseGuard.Infrastructure.JwtService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LicenseGuard.Infrastructure.JwtService;

public static class DependencyInjection
{
    public static IServiceCollection AddLicenseGuardJwtTokenService(
        this IServiceCollection services,
        Func<IServiceProvider, JwtTokenConfiguration> configurationFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationFactory);

        services.AddSingleton<JwtTokenConfiguration>(configurationFactory);
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        return services;
    }
}
