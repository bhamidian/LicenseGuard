using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Infrstructure.SecurityService.Contracts;

public interface ILicenseSigner
{
    LicenseSignature Sign(LicenseSigningData data);
    bool Verify(LicenseSigningData data, LicenseSignature signature);
}
