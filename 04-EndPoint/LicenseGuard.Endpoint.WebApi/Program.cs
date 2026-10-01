using LicenseGuard.Endpoint.WebApi.Configuration;
using LicenseGuard.Endpoint.WebApi.DependencyInjection;
using LicenseGuard.Endpoint.WebApi.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddLicenseGuardConfiguration(builder.Configuration)
    .AddLicenseGuardApplication()
    .AddLicenseGuardPersistence()
    .AddLicenseGuardIdentity()
    .AddLicenseGuardAuthentication()
    .AddLicenseGuardSecurity()
    .AddLicenseGuardRateLimiting()
    .AddLicenseGuardApi();

var app = builder.Build();
app.UseLicenseGuardPipeline();
await app.SeedDatabaseAsync();

app.Run();
