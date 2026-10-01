using LicenseGuard.Domain.Entities;
using LicenseGuard.Endpoint.WebApi.Configuration;
using LicenseGuard.Infrastructure.JwtService.Contracts;
using LicenseGuard.Infrstructure.SecurityService;
using LicenseGuard.Infrstructure.SecurityService.Contracts;
using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddLicenseGuardSecurity(this IServiceCollection services)
    {
        services.AddSingleton<ILicenseKeyProvider, CsprngLicenseKeyProvider>();
        services.AddSingleton<ILicenseSigner>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<LicenseSigningOptions>>().Value;
            return new RsaPssLicenseSigner(options.PrivateKeyPem, options.PublicKeyPem);
        });
        return services;
    }
}
