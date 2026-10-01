using LicenseGuard.Application.Contracts;
using LicenseGuard.Domain.ValueObjects;
using LicenseGuard.Infrstructure.SecurityService.Contracts;

namespace LicenseGuard.Application.Features.License.Services;

public sealed class LicenseService(ILicenseKeyProvider keyProvider) : ILicenseService
{
    public LicenseKey GenerateKey() => keyProvider.Generate();
}
