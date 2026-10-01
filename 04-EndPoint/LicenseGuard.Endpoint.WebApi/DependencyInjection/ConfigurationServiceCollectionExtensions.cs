using LicenseGuard.Endpoint.WebApi.Configuration;

namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public static class ConfigurationServiceCollectionExtensions
{
    public static IServiceCollection AddLicenseGuardConfiguration(
        this IServiceCollection services,
        IConfiguration configuration) =>
        LicenseGuardConfigurationExtensions.RegisterOptions(services, configuration);
}
