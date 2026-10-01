using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Domain.Repositories;

public interface ILicenseRepository
{
    License Create(CreateLicenseRecord record, Subscription subscription);
    Task<License?> GetForActivationAsync(LicenseKeyLookupRecord record, CancellationToken cancellationToken = default);
    Task<License?> GetForValidationAsync(LicenseKeyLookupRecord record, CancellationToken cancellationToken = default);
}
