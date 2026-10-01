using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Application.Contracts;

public interface ILicenseService
{
    LicenseKey GenerateKey();
}
