using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Domain.Records;

public sealed record CreateLicenseRecord(
    Guid SubscriptionId,
    Guid IssuedByAdminId,
    LicenseKey Key,
    LicenseStatusEnum Status,
    DateTime StartDate,
    DateTime ExpirationDate,
    int MaxActivations,
    Guid CustomerId,
    Guid ProductId,
    Guid PlanId,
    string PolicyVersion = "1",
    IReadOnlyCollection<Guid>? FeatureIds = null);
