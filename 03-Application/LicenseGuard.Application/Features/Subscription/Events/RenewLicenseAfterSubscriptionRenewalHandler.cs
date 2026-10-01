using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrstructure.SecurityService.Contracts;
using LicenseGuard.Application.Features.Subscription.Records;
using MediatR;

namespace LicenseGuard.Application.Features.Subscription.Events;

public sealed class RenewLicenseAfterSubscriptionRenewalHandler(
    ISubscriptionRepository subscriptions,
    ILicenseSigner signer)
    : INotificationHandler<SubscriptionRenewedNotification>
{
    public async Task Handle(SubscriptionRenewedNotification notification, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetForRenewalAsync(notification.SubscriptionId, cancellationToken)
            ?? throw new InvalidOperationException("The renewed subscription could not be reloaded.");

        var license = subscription.CurrentLicense;
        if (license is null) return;

        license.Renew(notification.NewExpirationDate, notification.Amount,
            notification.RenewedByUserId, notification.RenewedAt);
        license.SetSignature(signer.Sign(license.GetSigningData()));
    }
}
