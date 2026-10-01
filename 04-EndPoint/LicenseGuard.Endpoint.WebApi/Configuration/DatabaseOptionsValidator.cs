using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.Configuration;

public sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            return ValidateOptionsResult.Success;

        if (!options.Provider.Equals("mysql", StringComparison.OrdinalIgnoreCase)
            && !options.Provider.Equals("mariadb", StringComparison.OrdinalIgnoreCase))
            return ValidateOptionsResult.Fail("Database:Provider must be either 'mysql' or 'mariadb'.");

        if (!string.IsNullOrWhiteSpace(options.ServerVersion)
            && !Version.TryParse(options.ServerVersion, out _))
            return ValidateOptionsResult.Fail("Database:ServerVersion must be a valid version number.");

        return ValidateOptionsResult.Success;
    }
}
