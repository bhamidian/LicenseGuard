using LicenseGuard.Domain.Enums;

namespace LicenseGuard.Domain.Records;

public sealed record LicenseSearchCriteria(
    string? LicenseKey,
    Guid? CustomerId,
    Guid? ProductId,
    Guid? PlanId,
    LicenseStatusEnum? Status,
    DateTime? ExpiresAfter,
    DateTime? ExpiresBefore,
    int Skip,
    int Take);

public sealed record LicenseSearchItemRecord(
    Guid LicenseId,
    string LicenseKey,
    Guid CustomerId,
    Guid ProductId,
    Guid PlanId,
    LicenseStatusEnum Status,
    DateTime CreatedAt,
    DateTime StartDate,
    DateTime ExpirationDate,
    int MaxActivations,
    int ActiveActivations,
    int EnabledFeatureCount,
    bool AutoRenewalEnabled);

public sealed record LicenseSearchPageRecord(
    IReadOnlyCollection<LicenseSearchItemRecord> Items,
    int TotalCount);
