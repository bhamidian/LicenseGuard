using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using LicenseGuard.Infrastructure.JwtService.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace LicenseGuard.Infrastructure.JwtService.Services;

public sealed class JwtTokenService : IJwtTokenService, IDisposable
{
    private readonly JwtTokenConfiguration _options;
    private readonly RSA _privateRsaKey;

    public JwtTokenService(JwtTokenConfiguration options)
    {
        _options = options;
        ValidateOptions(_options);

        _privateRsaKey = RSA.Create();
        try
        {
            _privateRsaKey.ImportFromPem(_options.PrivateKeyPem);
            if (_privateRsaKey.KeySize < 2048)
                throw new InvalidOperationException("JWT RSA signing keys must be at least 2048 bits.");
        }
        catch
        {
            _privateRsaKey.Dispose();
            throw;
        }
    }

    public JwtTokenResponse CreateAccessToken(JwtTokenRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.UserId == Guid.Empty)
            throw new ArgumentException("A valid user ID is required.", nameof(request));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserName);
        ArgumentNullException.ThrowIfNull(request.Roles);

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId.ToString("D")),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.NameIdentifier, request.UserId.ToString("D")),
            new(ClaimTypes.Name, request.UserName.Trim())
        };

        if (!string.IsNullOrWhiteSpace(request.Email))
            claims.Add(new Claim(ClaimTypes.Email, request.Email.Trim()));

        claims.AddRange(request.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => new Claim(ClaimTypes.Role, role.Trim())));

        var credentials = new SigningCredentials(
            new RsaSecurityKey(_privateRsaKey),
            SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtTokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public void Dispose() => _privateRsaKey.Dispose();

    private static void ValidateOptions(JwtTokenConfiguration options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Audience);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.PrivateKeyPem);
        if (options.AccessTokenLifetimeMinutes is < 1 or > 1440)
            throw new InvalidOperationException("JWT access token lifetime must be between 1 minute and 24 hours.");
    }
}
