using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using LicenseGuard.Infrastructure.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LicenseGuard.Infrastructure.EFCore;

public static class DependencyInjection
{
    public static IServiceCollection AddLicenseGuardPersistence(
        this IServiceCollection services,
        string connectionString,
        string provider = "mysql",
        string? serverVersion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var version = Version.TryParse(serverVersion, out var parsedVersion)
            ? parsedVersion
            : provider.Equals("mariadb", StringComparison.OrdinalIgnoreCase)
                ? new Version(10, 11, 0)
                : new Version(8, 0, 29);
        ServerVersion dbVersion = provider.Equals("mariadb", StringComparison.OrdinalIgnoreCase)
            ? new MariaDbServerVersion(version)
            : new MySqlServerVersion(version);

        services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString, dbVersion));
        return RegisterRepositories(services);
    }

    public static IServiceCollection AddLicenseGuardPersistence(
        this IServiceCollection services,
        Func<IServiceProvider, (string ConnectionString, string Provider, string? ServerVersion)?> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(settingsFactory);
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var settings = settingsFactory(serviceProvider);
            if (settings is null)
                return;

            var databaseSettings = settings.Value;
            var version = Version.TryParse(databaseSettings.ServerVersion, out var parsedVersion)
                ? parsedVersion
                : databaseSettings.Provider.Equals("mariadb", StringComparison.OrdinalIgnoreCase)
                    ? new Version(10, 11, 0)
                    : new Version(8, 0, 29);
            ServerVersion serverVersion = databaseSettings.Provider.Equals("mariadb", StringComparison.OrdinalIgnoreCase)
                ? new MariaDbServerVersion(version)
                : new MySqlServerVersion(version);
            options.UseMySql(databaseSettings.ConnectionString, serverVersion);
        });

        return RegisterRepositories(services);
    }

    private static IServiceCollection RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<ISubscriptionRepository, EfSubscriptionRepository>();
        services.AddScoped<IAdminRepository, EfAdminRepository>();
        services.AddScoped<IProductCatalogRepository, EfProductCatalogRepository>();
        services.AddScoped<IAppUserRepository, EfAppUserRepository>();
        services.AddScoped<ICustomerRepository, EfCustomerRepository>();
        services.AddScoped<ISubscriptionRenewalRepository, EfSubscriptionRenewalRepository>();
        services.AddScoped<ILicenseRepository, EfLicenseRepository>();
        services.AddScoped<ILicenseActivationRepository, EfLicenseActivationRepository>();
        services.AddScoped<IAuditLogRepository, EfAuditLogRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        return services;
    }
}
