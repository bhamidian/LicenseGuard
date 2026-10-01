namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddLicenseGuardApplication(this IServiceCollection services)
    {
        global::LicenseGuard.Application.DependencyInjection.AddLicenseGuardApplication(services);
        return services;
    }
}
