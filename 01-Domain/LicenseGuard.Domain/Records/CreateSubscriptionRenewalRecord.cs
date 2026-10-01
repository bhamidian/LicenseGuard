namespace LicenseGuard.Domain.Records;

public sealed record CreateSubscriptionRenewalRecord(
    Guid SubscriptionId,
    Guid RenewedByUserId,
    decimal Amount,
    DateTime PreviousExpirationDate,
    DateTime NewExpirationDate,
    DateTime? RenewedAt = null);
