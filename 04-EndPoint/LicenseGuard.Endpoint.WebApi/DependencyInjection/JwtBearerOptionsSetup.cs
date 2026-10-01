using System.Security.Claims;
using System.Security.Cryptography;
using LicenseGuard.Endpoint.WebApi.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LicenseGuard.Endpoint.WebApi.DependencyInjection;

public sealed class JwtBearerOptionsSetup(IOptions<JwtTokenOptions> tokenOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options) => Configure(JwtBearerDefaults.AuthenticationScheme, options);

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (!string.Equals(name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal))
            return;

        var jwt = tokenOptions.Value;
        var publicKeyPem = jwt.PublicKeyPem;
        if (string.IsNullOrWhiteSpace(publicKeyPem)
            && !string.IsNullOrWhiteSpace(jwt.PrivateKeyPem))
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(jwt.PrivateKeyPem);
            publicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();
        }

        if (string.IsNullOrWhiteSpace(publicKeyPem))
            return;

        using var publicRsa = RSA.Create();
        publicRsa.ImportFromPem(publicKeyPem);

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(publicRsa.ExportParameters(false)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    }
}
