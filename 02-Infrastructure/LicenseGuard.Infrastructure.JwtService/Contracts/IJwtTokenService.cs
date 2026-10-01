namespace LicenseGuard.Infrastructure.JwtService.Contracts;

public interface IJwtTokenService
{
    JwtTokenResponse CreateAccessToken(JwtTokenRequest request);
}
