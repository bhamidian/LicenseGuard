namespace LicenseGuard.Endpoint.WebApi.Configuration;

public sealed class JwtTokenOptions
{
    public const string SectionName = "JwtToken";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string PrivateKeyPem { get; set; } = string.Empty;
    public string PublicKeyPem { get; set; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; set; }
}
