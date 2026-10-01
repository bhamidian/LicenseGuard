namespace LicenseGuard.Infrastructure.JwtService.Contracts;

public sealed record JwtTokenConfiguration(
    string Issuer,
    string Audience,
    string PrivateKeyPem,
    int AccessTokenLifetimeMinutes);
