using System.Security.Cryptography;
using LicenseGuard.Domain.ValueObjects;
using LicenseGuard.Infrstructure.SecurityService.Contracts;

namespace LicenseGuard.Infrstructure.SecurityService;

/// <summary>Generates 256-bit license keys using the operating-system CSPRNG.</summary>
public sealed class CsprngLicenseKeyProvider : ILicenseKeyProvider
{
    public LicenseKey Generate() => LicenseKey.Create(Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
}
