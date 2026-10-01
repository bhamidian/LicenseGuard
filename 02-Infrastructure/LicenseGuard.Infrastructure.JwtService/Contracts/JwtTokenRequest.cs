namespace LicenseGuard.Infrastructure.JwtService.Contracts;

public sealed record JwtTokenRequest(
    Guid UserId,
    string UserName,
    IReadOnlyCollection<string> Roles,
    string? Email = null);
