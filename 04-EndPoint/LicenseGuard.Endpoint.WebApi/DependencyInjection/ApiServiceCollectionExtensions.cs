using Microsoft.AspNetCore.Mvc;
using LicenseGuard.Endpoint.WebApi.Workers;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddLicenseGuardApi(this IServiceCollection services)
    {
        services.AddHostedService<LicenseExpirationWorker>();
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LicenseGuard API",
                Version = "v1"
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter a JWT access token."
            });
            options.OperationFilter<SwaggerAuthorizationOperationFilter>();
        });
        return services;
    }

    public static WebApplication UseLicenseGuardPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "LicenseGuard API v1"));
        }

        app.UseHttpsRedirection();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
