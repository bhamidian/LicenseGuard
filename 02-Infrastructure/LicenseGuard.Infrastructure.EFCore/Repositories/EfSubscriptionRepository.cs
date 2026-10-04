using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfSubscriptionRepository(ApplicationDbContext dbContext) : ISubscriptionRepository
{
    public Task<Subscription?> GetForLicenseIssuanceAsync(Guid subscriptionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Subscriptions
            .FromSqlInterpolated($"SELECT * FROM `Subscriptions` WHERE `Id` = {subscriptionId} FOR UPDATE")
            .Include(subscription => subscription.Product)
            .Include(subscription => subscription.Plan)
                .ThenInclude(plan => plan.PlanFeatures)
                    .ThenInclude(planFeature => planFeature.Feature)
            .SingleOrDefaultAsync(subscription => subscription.Id == subscriptionId, cancellationToken);

    public Task<Subscription?> GetForRenewalAsync(Guid subscriptionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Subscriptions
            .FromSqlInterpolated($"SELECT * FROM `Subscriptions` WHERE `Id` = {subscriptionId} FOR UPDATE")
            .Include(subscription => subscription.CurrentLicense)
                .ThenInclude(license => license!.LicenseFeatures)
            .Include(subscription => subscription.CurrentLicense)
                .ThenInclude(license => license!.Limits)
            .SingleOrDefaultAsync(subscription => subscription.Id == subscriptionId, cancellationToken);

    public async Task<Subscription?> GetDetailsAsync(Guid subscriptionId, Guid? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Subscriptions.AsNoTracking()
            .Where(subscription => subscription.Id == subscriptionId);
        if (customerId is { } ownerId)
            query = query.Where(subscription => subscription.CustomerId == ownerId);

        return await query
            .Include(subscription => subscription.Product)
            .Include(subscription => subscription.Plan)
            .Include(subscription => subscription.Renewals)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);
    }
}
