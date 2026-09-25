using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Domain.Interfaces
{
    public interface ILicenseSigner
    {
        LicenseSignature Sign(LicenseSigningData data);

        bool Verify(
            LicenseSigningData data,
            LicenseSignature signature);
    }
}