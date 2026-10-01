using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Domain.Repositories;

public interface ILicenseActivationRepository
{
    LicenseActivation Create(CreateLicenseActivationRecord record);
}
