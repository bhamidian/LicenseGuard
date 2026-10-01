using MediatR;

namespace LicenseGuard.Application.Features.Subscription.Records;

public sealed record SubscriptionRenewedNotification(
    Guid SubscriptionId,
    Guid RenewedByUserId,
    decimal Amount,
    DateTime NewExpirationDate,
    DateTime RenewedAt) : INotification;
