using FluentValidation;
using LicenseGuard.Application.Contracts;
using LicenseGuard.Application.Features.License.Services;
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
    IPublisher publisher,
    IAuditLogRepository auditLogs,
    IAuditLogService auditLogService)
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

        var previousExpirationDate = subscription.EndDate;
        var renewal = renewals.Create(new CreateSubscriptionRenewalRecord(
            subscription.Id, command.RenewedByUserId, command.Amount, previousExpirationDate, command.NewExpirationDate));
        subscription.Renew(renewal, command.RenewedByUserId);

        await publisher.Publish(new SubscriptionRenewedNotification(subscription.Id, command.RenewedByUserId,
            renewal.Amount, renewal.NewExpirationDate, renewal.RenewedAt), cancellationToken);

        var auditRecord = await auditLogService.ValidateSubscriptionRenewedAsync(subscription.Id,
            command.RenewedByUserId, subscription.CurrentLicense?.Id, renewal.Amount,
            previousExpirationDate, renewal.NewExpirationDate, cancellationToken);
        if (!auditRecord.IsSuccess || auditRecord.Data is null)
            return ResultDto<RenewSubscriptionResponse>.Fail(auditRecord.Message, auditRecord.Errors,
                auditRecord.FailureKind ?? ResultFailureKind.Validation);
        if (subscription.CurrentLicense is { } linkedLicense)
            auditLogs.Create(auditRecord.Data, linkedLicense);
        else
            auditLogs.Create(auditRecord.Data);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var license = subscription.CurrentLicense;
        return ResultDto<RenewSubscriptionResponse>.Success("Subscription renewed.",
            new RenewSubscriptionResponse(subscription.Id, license?.Id, subscription.EndDate,
                license?.ExpirationDate, license?.Signature?.Value));
    }
}
