namespace LicenseGuard.Infrastructure.JwtService.Contracts;

public sealed record JwtTokenResponse(string AccessToken, DateTime ExpiresAtUtc);
