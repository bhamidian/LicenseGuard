using LicenseGuard.Domain.Entities;

namespace LicenseGuard.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetForLicenseIssuanceAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetForRenewalAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetDetailsAsync(Guid subscriptionId, Guid? customerId = null,
        CancellationToken cancellationToken = default);
}
