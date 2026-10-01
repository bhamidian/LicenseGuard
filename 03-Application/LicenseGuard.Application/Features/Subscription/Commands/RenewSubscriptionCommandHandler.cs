using FluentValidation;
using LicenseGuard.Application.Contracts;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Application.Features.Subscription.Records;
using MediatR;
using System.Data;

namespace LicenseGuard.Application.Features.Subscription.Commands;

public sealed class RenewSubscriptionCommandHandler(
    IValidator<RenewSubscriptionCommand> validator,
    ISubscriptionRepository subscriptions,
    ISubscriptionRenewalRepository renewals,
    IUnitOfWork unitOfWork,
    IPublisher publisher)
    : IRequestHandler<RenewSubscriptionCommand, ResultDto<RenewSubscriptionResponse>>
{
    public async Task<ResultDto<RenewSubscriptionResponse>> Handle(
        RenewSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<RenewSubscriptionResponse>.Fail("Subscription renewal validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));

        await using var transaction = await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var subscription = await subscriptions.GetForRenewalAsync(command.SubscriptionId, cancellationToken);
        if (subscription is null)
            return ResultDto<RenewSubscriptionResponse>.Fail("Subscription was not found.", failureKind: ResultFailureKind.NotFound);

        var renewal = renewals.Create(new CreateSubscriptionRenewalRecord(
            subscription.Id, command.RenewedByUserId, command.Amount, subscription.EndDate, command.NewExpirationDate));
        subscription.Renew(renewal, command.RenewedByUserId);

        await publisher.Publish(new SubscriptionRenewedNotification(subscription.Id, command.RenewedByUserId,
            renewal.Amount, renewal.NewExpirationDate, renewal.RenewedAt), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var license = subscription.CurrentLicense;
        return ResultDto<RenewSubscriptionResponse>.Success("Subscription renewed.",
            new RenewSubscriptionResponse(subscription.Id, license?.Id, subscription.EndDate,
                license?.ExpirationDate, license?.Signature?.Value));
    }
}
