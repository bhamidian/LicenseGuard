using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Infrstructure.SecurityService.Contracts;

public interface ILicenseKeyProvider
{
    LicenseKey Generate();
}
