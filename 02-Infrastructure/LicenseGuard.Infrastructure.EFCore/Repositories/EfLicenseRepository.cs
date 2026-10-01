using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Domain.Records;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfLicenseRepository(ApplicationDbContext dbContext) : ILicenseRepository
{
    public Task<License?> GetForActivationAsync(LicenseKeyLookupRecord record,
        CancellationToken cancellationToken = default) =>
        dbContext.Licenses
            .FromSqlInterpolated($"SELECT * FROM `Licenses` WHERE `Key` = {record.LicenseKey} FOR UPDATE")
            .Include(license => license.Subscription)
            .Include(license => license.Activations)
            .Include(license => license.LicenseFeatures)
            .Include(license => license.Limits)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<License?> GetForValidationAsync(LicenseKeyLookupRecord record,
        CancellationToken cancellationToken = default) =>
        dbContext.Licenses
            .FromSqlInterpolated($"SELECT * FROM `Licenses` WHERE `Key` = {record.LicenseKey}")
            .Include(license => license.Subscription)
            .Include(license => license.Activations)
            .Include(license => license.LicenseFeatures)
            .Include(license => license.Limits)
            .SingleOrDefaultAsync(cancellationToken);

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
