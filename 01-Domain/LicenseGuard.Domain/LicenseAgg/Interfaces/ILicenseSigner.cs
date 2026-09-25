using LicenseGuard.Domain.LicenseAgg.Records;
using LicenseGuard.Domain.LicenseAgg.ValueObjects;

namespace LicenseGuard.Domain.LicenseAgg.Interfaces
{
    public interface ILicenseSigner
    {
        LicenseSignature Sign(LicenseSigningData data);

        bool Verify(
            LicenseSigningData data,
            LicenseSignature signature);
    }
}