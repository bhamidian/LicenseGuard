using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.EFCore.Persistence;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfSubscriptionRenewalRepository(ApplicationDbContext dbContext) : ISubscriptionRenewalRepository
{
    public SubscriptionRenewal Create(CreateSubscriptionRenewalRecord record)
    {
        var renewal = new SubscriptionRenewal(record.SubscriptionId, record.Amount,
            record.PreviousExpirationDate, record.NewExpirationDate, record.RenewedAt);
        dbContext.SubscriptionRenewals.Add(renewal);
        return renewal;
    }
}
