using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Domain.Repositories;

public interface ISubscriptionRenewalRepository
{
    SubscriptionRenewal Create(CreateSubscriptionRenewalRecord record);
}
