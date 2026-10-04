using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Domain.Records;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using LicenseGuard.Domain.ValueObjects;
using LicenseGuard.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfLicenseRepository(ApplicationDbContext dbContext) : ILicenseRepository
{
    public async Task<License?> GetForStatusChangeAsync(Guid licenseId,
        CancellationToken cancellationToken = default)
    {
        var license = await dbContext.Licenses
            .FromSqlInterpolated($"SELECT * FROM `Licenses` WHERE `Id` = {licenseId} FOR UPDATE")
            .Include(license => license.Subscription)
            .Include(license => license.StatusHistory)
            .SingleOrDefaultAsync(cancellationToken);

        PreserveCurrentLicensePointer(license);

        return license;
    }

    public async Task<License?> GetForActivationDeactivationAsync(Guid licenseId,
        CancellationToken cancellationToken = default)
    {
        var license = await dbContext.Licenses
            .FromSqlInterpolated($"SELECT * FROM `Licenses` WHERE `Id` = {licenseId} FOR UPDATE")
            .Include(entity => entity.Activations)
            .SingleOrDefaultAsync(cancellationToken);

        PreserveCurrentLicensePointer(license);
        return license;
    }

    public async Task<License?> GetForUpdateAsync(Guid licenseId,
        CancellationToken cancellationToken = default)
    {
        var license = await dbContext.Licenses
            .FromSqlInterpolated($"SELECT * FROM `Licenses` WHERE `Id` = {licenseId} FOR UPDATE")
            .Include(entity => entity.Subscription)
            .Include(entity => entity.LicenseFeatures)
            .Include(entity => entity.Limits)
            .Include(entity => entity.Activations)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        PreserveCurrentLicensePointer(license);
        return license;
    }

    public async Task<IReadOnlyCollection<Feature>> GetAvailableFeaturesForPlanAsync(Guid planId,
        CancellationToken cancellationToken = default) =>
        await dbContext.PlanFeatures
            .Where(link => link.PlanId == planId && link.IsActive && !link.IsDeleted &&
                link.Feature.IsActive && !link.Feature.IsDeleted)
            .Select(link => link.Feature)
            .Include(feature => feature.ProductFeatures)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> GetExpiredLicenseIdsAsync(DateTime asOf, int batchSize,
        CancellationToken cancellationToken = default) =>
        await dbContext.Licenses
            .AsNoTracking()
            .Where(license => license.ExpirationDate <= asOf &&
                (license.LicenseStatus == LicenseStatusEnum.PENDING ||
                 license.LicenseStatus == LicenseStatusEnum.ACTIVE ||
                 license.LicenseStatus == LicenseStatusEnum.SUSPENDED))
            .OrderBy(license => license.ExpirationDate)
            .Select(license => license.Id)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);

    public void AddStatusHistory(LicenseStatusHistory history) =>
        dbContext.Entry(history).State = EntityState.Added;

    public async Task<LicenseSearchPageRecord> SearchAsync(LicenseSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Licenses.AsNoTracking().AsQueryable();
        if (criteria.LicenseKey is not null)
        {
            var key = LicenseKey.Create(criteria.LicenseKey);
            query = query.Where(license => license.Key == key);
        }
        if (criteria.CustomerId is { } customerId)
            query = query.Where(license => license.CustomerId == customerId);
        if (criteria.ProductId is { } productId)
            query = query.Where(license => license.ProductId == productId);
        if (criteria.PlanId is { } planId)
            query = query.Where(license => license.PlanId == planId);
        if (criteria.Status is { } status)
            query = query.Where(license => license.LicenseStatus == status);
        if (criteria.ExpiresAfter is { } expiresAfter)
            query = query.Where(license => license.ExpirationDate >= expiresAfter);
        if (criteria.ExpiresBefore is { } expiresBefore)
            query = query.Where(license => license.ExpirationDate <= expiresBefore);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(license => license.CreatedAt)
            .ThenBy(license => license.Id)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .Select(license => new LicenseSearchItemRecord(
                license.Id,
                license.Key.Value,
                license.CustomerId,
                license.ProductId,
                license.PlanId,
                license.LicenseStatus,
                license.CreatedAt,
                license.StartDate,
                license.ExpirationDate,
                license.Limits.Where(limit => limit.Code == "max_activations")
                    .Select(limit => (int?)limit.Value).FirstOrDefault() ?? 1,
                license.Activations.Count(activation => activation.Status == ActivationStatusEnum.Active),
                license.LicenseFeatures.Count(feature => feature.IsEnabled),
                license.AutoRenewal.IsEnabled))
            .ToArrayAsync(cancellationToken);

        return new LicenseSearchPageRecord(items, totalCount);
    }

    public Task<License?> GetDetailsAsync(Guid licenseId, CancellationToken cancellationToken = default) =>
        dbContext.Licenses
            .AsNoTracking()
            .Where(license => license.Id == licenseId)
            .Include(license => license.Subscription).ThenInclude(subscription => subscription.Product)
            .Include(license => license.Subscription).ThenInclude(subscription => subscription.Plan)
            .Include(license => license.Activations)
            .Include(license => license.LicenseFeatures)
            .Include(license => license.Limits)
            .Include(license => license.StatusHistory)
            .Include(license => license.AuditLogs)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

    public Task<License?> GetForActivationAsync(LicenseKeyLookupRecord record,
        CancellationToken cancellationToken = default) => GetForActivationWithSubscriptionAsync(record, cancellationToken);

    private async Task<License?> GetForActivationWithSubscriptionAsync(LicenseKeyLookupRecord record,
        CancellationToken cancellationToken)
    {
        var license = await dbContext.Licenses
            .FromSqlInterpolated($"SELECT * FROM `Licenses` WHERE `Key` = {record.LicenseKey} FOR UPDATE")
            .Include(license => license.Subscription)
            .Include(license => license.Activations)
            .Include(license => license.LicenseFeatures)
            .Include(license => license.Limits)
            .SingleOrDefaultAsync(cancellationToken);
        PreserveCurrentLicensePointer(license);
        return license;
    }

    public async Task<License?> GetForValidationAsync(LicenseKeyLookupRecord record,
        CancellationToken cancellationToken = default)
    {
        var license = await dbContext.Licenses
            .FromSqlInterpolated($"SELECT * FROM `Licenses` WHERE `Key` = {record.LicenseKey}")
            .Include(license => license.Subscription)
            .Include(license => license.Activations)
            .Include(license => license.LicenseFeatures)
            .Include(license => license.Limits)
            .SingleOrDefaultAsync(cancellationToken);
        PreserveCurrentLicensePointer(license);
        return license;
    }

    private void PreserveCurrentLicensePointer(License? license)
    {
        if (license?.Subscription is not { } subscription) return;
        var currentLicense = dbContext.Entry(subscription).Property(entity => entity.CurrentLicenseId);
        currentLicense.OriginalValue = currentLicense.CurrentValue;
        currentLicense.IsModified = false;
    }

    public License Create(CreateLicenseRecord record, Subscription subscription)
    {

        var license = new License(record.SubscriptionId, record.IssuedByAdminId, record.Key,
            record.Status, record.StartDate, record.ExpirationDate, record.MaxActivations,
            record.CustomerId, record.ProductId, record.PlanId, record.PolicyVersion);
        var selectedFeatureIds = record.FeatureIds?.ToHashSet();
        foreach (var feature in subscription.Plan.PlanFeatures.Select(x => x.Feature)
                     .Where(feature => selectedFeatureIds is null || selectedFeatureIds.Contains(feature.Id))
                     .OrderBy(x => x.Id))
            license.AddSnapshotFeature(feature);
        subscription.AddLicense(license);
        dbContext.Licenses.Add(license);
        return license;
    }
}
