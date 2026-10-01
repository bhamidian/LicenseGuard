using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.Configuration;

public sealed class LicenseSigningOptionsValidator : IValidateOptions<LicenseSigningOptions>
{
    public ValidateOptionsResult Validate(string? name, LicenseSigningOptions options)
    {
        var failures = new List<string>();
        ValidatePem(options.PrivateKeyPem, "LicenseSigning:PrivateKeyPem", failures);
        ValidatePem(options.PublicKeyPem, "LicenseSigning:PublicKeyPem", failures);
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
