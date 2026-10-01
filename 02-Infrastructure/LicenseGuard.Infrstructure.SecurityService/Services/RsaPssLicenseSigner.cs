using System.Security.Cryptography;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.ValueObjects;
using LicenseGuard.Infrstructure.SecurityService.Contracts;

namespace LicenseGuard.Infrstructure.SecurityService;

/// <summary>RSA-PSS/SHA-256 signer. The private PEM is only used for signing;
/// clients need only the corresponding public PEM to verify licenses.</summary>
public sealed class RsaPssLicenseSigner(string? privateKeyPem, string? publicKeyPem) : ILicenseSigner
{
    private const string SignaturePrefix = "rsa-pss-sha256:v2:";

    public LicenseSignature Sign(LicenseSigningData data)
    {
        if (string.IsNullOrWhiteSpace(privateKeyPem))
            throw new InvalidOperationException("License signing private key is not configured.");

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        var signature = rsa.SignData(LicenseSigningDataCanonicalizer.Canonicalize(data),
            HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        return LicenseSignature.Create(SignaturePrefix + Convert.ToBase64String(signature));
    }

    public bool Verify(LicenseSigningData data, LicenseSignature signature)
    {
        ArgumentNullException.ThrowIfNull(signature);
        if (string.IsNullOrWhiteSpace(publicKeyPem) || !signature.Value.StartsWith(SignaturePrefix, StringComparison.Ordinal))
            return false;

        try
        {
            var bytes = Convert.FromBase64String(signature.Value[SignaturePrefix.Length..]);
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            return rsa.VerifyData(LicenseSigningDataCanonicalizer.Canonicalize(data), bytes,
                HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}
