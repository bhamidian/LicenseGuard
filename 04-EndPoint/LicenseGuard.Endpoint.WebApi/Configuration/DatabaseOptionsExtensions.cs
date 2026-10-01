namespace LicenseGuard.Endpoint.WebApi.Configuration;

public static class DatabaseOptionsExtensions
{
    public static bool IsConfigured(this DatabaseOptions options) =>
        !string.IsNullOrWhiteSpace(options.ConnectionString);
}
