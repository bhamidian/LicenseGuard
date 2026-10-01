namespace LicenseGuard.Endpoint.WebApi.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string? ConnectionString { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? ServerVersion { get; set; }
}
