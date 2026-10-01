using LicenseGuard.Domain.Entities;
using LicenseGuard.Endpoint.WebApi.Configuration;
using LicenseGuard.Infrastructure.EFCore;
using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddLicenseGuardPersistence(this IServiceCollection services)
    {
        services.AddLicenseGuardPersistence(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            return options.IsConfigured()
                ? (options.ConnectionString!, options.Provider, options.ServerVersion)
                : null;
        });
        return services;
    }
}
