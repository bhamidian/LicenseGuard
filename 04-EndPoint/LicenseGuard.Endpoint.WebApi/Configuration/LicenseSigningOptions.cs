namespace LicenseGuard.Endpoint.WebApi.Configuration;

public sealed class LicenseSigningOptions
{
    public const string SectionName = "LicenseSigning";

    public string? PrivateKeyPem { get; set; }
    public string? PublicKeyPem { get; set; }
}
