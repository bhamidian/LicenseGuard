using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.Configuration;

public static class LicenseGuardConfigurationExtensions
{
    public static IServiceCollection RegisterOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatedOptions<DatabaseOptions, DatabaseOptionsValidator>(configuration, DatabaseOptions.SectionName);
        services.AddValidatedOptions<JwtTokenOptions, JwtTokenOptionsValidator>(configuration, JwtTokenOptions.SectionName);
        services.AddValidatedOptions<LicenseSigningOptions, LicenseSigningOptionsValidator>(configuration, LicenseSigningOptions.SectionName);
        services.AddBoundOptions<IdentityPolicyOptions>(configuration, IdentityPolicyOptions.SectionName);
        services.AddBoundOptions<RateLimitOptions>(configuration, RateLimitOptions.SectionName);
        return services;
    }

    private static void AddValidatedOptions<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
        where TValidator : class, IValidateOptions<TOptions>
    {
        services.AddSingleton<IValidateOptions<TOptions>, TValidator>();
        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateOnStart();
    }

    private static void AddBoundOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class
    {
        services.AddOptions<TOptions>().Bind(configuration.GetSection(sectionName));
    }
}
