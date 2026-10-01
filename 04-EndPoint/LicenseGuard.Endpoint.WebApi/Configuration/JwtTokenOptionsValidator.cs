using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.Configuration;

public sealed class JwtTokenOptionsValidator : IValidateOptions<JwtTokenOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtTokenOptions options)
    {
        var failures = new List<string>();
        if (string.IsNullOrWhiteSpace(options.Issuer))
            failures.Add("JwtToken:Issuer is required.");
        if (string.IsNullOrWhiteSpace(options.Audience))
            failures.Add("JwtToken:Audience is required.");
        if (options.AccessTokenLifetimeMinutes is < 1 or > 1440)
            failures.Add("JwtToken:AccessTokenLifetimeMinutes must be between 1 and 1440.");

        ValidatePem(options.PrivateKeyPem, "JwtToken:PrivateKeyPem", failures);
        ValidatePem(options.PublicKeyPem, "JwtToken:PublicKeyPem", failures);
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidatePem(string? pem, string settingName, ICollection<string> failures)
    {
        if (string.IsNullOrWhiteSpace(pem))
            return;

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            if (rsa.KeySize < 2048)
                failures.Add($"{settingName} must contain an RSA key of at least 2048 bits.");
        }
        catch (Exception exception) when (exception is CryptographicException or ArgumentException)
        {
            failures.Add($"{settingName} must contain a valid RSA PEM key.");
        }
    }
}
