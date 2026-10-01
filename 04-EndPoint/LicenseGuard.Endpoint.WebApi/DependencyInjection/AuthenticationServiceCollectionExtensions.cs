using LicenseGuard.Domain.Entities;
using LicenseGuard.Endpoint.WebApi.Configuration;
using LicenseGuard.Infrastructure.JwtService;
using LicenseGuard.Infrastructure.JwtService.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddLicenseGuardAuthentication(this IServiceCollection services)
    {
        services.AddLicenseGuardJwtTokenService(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<JwtTokenOptions>>().Value;
            return new JwtTokenConfiguration(
                options.Issuer,
                options.Audience,
                options.PrivateKeyPem,
                options.AccessTokenLifetimeMinutes);
        });
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();
        services.AddAuthorization();
        return services;
    }
}
