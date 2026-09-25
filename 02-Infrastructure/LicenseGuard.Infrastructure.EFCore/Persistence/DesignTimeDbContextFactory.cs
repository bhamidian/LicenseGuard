using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LicenseGuard.Infrastructure.EFCore.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("LICENSEGUARD_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Set LICENSEGUARD_CONNECTION_STRING before running EF Core design-time commands.");

        var providerName = Environment.GetEnvironmentVariable("LICENSEGUARD_DB_PROVIDER")
            ?? "mysql";

        var versionText = Environment.GetEnvironmentVariable("LICENSEGUARD_DB_VERSION");
        var version = Version.TryParse(versionText, out var parsedVersion)
            ? parsedVersion
            : providerName.Equals("mariadb", StringComparison.OrdinalIgnoreCase)
                ? new Version(10, 11, 0)
                : new Version(8, 0, 29);

        ServerVersion serverVersion = providerName.Equals("mariadb", StringComparison.OrdinalIgnoreCase)
            ? new MariaDbServerVersion(version)
            : new MySqlServerVersion(version);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, serverVersion)
            .Options;

        return new ApplicationDbContext(options);
    }
}
