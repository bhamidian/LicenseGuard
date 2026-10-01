using LicenseGuard.Endpoint.WebApi.Configuration;
using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.Identity;

public static class IdentityStartupInitializer
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        var databaseOptions = app.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        if (databaseOptions.IsConfigured())
            await IdentitySeeder.SeedAsync(app.Services);
    }
}
