using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Domain.Repositories;

public interface ILicenseRepository
{
    License Create(CreateLicenseRecord record, Subscription subscription);
    Task<License?> GetForStatusChangeAsync(Guid licenseId, CancellationToken cancellationToken = default);
    Task<License?> GetForActivationDeactivationAsync(Guid licenseId, CancellationToken cancellationToken = default);
    Task<License?> GetForUpdateAsync(Guid licenseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Feature>> GetAvailableFeaturesForPlanAsync(Guid planId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> GetExpiredLicenseIdsAsync(DateTime asOf, int batchSize,
        CancellationToken cancellationToken = default);
    void AddStatusHistory(LicenseStatusHistory history);
    Task<LicenseSearchPageRecord> SearchAsync(LicenseSearchCriteria criteria,
        CancellationToken cancellationToken = default);
    Task<License?> GetDetailsAsync(Guid licenseId, CancellationToken cancellationToken = default);
    Task<License?> GetForActivationAsync(LicenseKeyLookupRecord record, CancellationToken cancellationToken = default);
    Task<License?> GetForValidationAsync(LicenseKeyLookupRecord record, CancellationToken cancellationToken = default);
}
