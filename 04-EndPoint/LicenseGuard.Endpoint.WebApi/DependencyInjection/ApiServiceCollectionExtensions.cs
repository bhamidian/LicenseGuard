using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddLicenseGuardApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        return services;
    }

    public static WebApplication UseLicenseGuardPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseHttpsRedirection();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
