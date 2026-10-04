using FluentValidation;
using LicenseGuard.Application.Contracts;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Application.Features.License.Services;
using LicenseGuard.Infrstructure.SecurityService.Contracts;
using MediatR;
using System.Data;

namespace LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand;

public sealed class CreateLicenseCommandHandler(
    IValidator<CreateLicenseCommand> validator,
    ILicenseService licenseService,
    ISubscriptionRepository subscriptions,
    ILicenseRepository licenses,
    IAuditLogRepository auditLogs,
    IUnitOfWork unitOfWork,
    ILicenseSigner signer,
    IAuditLogService auditLogService)
    : IRequestHandler<CreateLicenseCommand, ResultDto<CreateLicenseResponse>>
{
    public async Task<ResultDto<CreateLicenseResponse>> Handle(CreateLicenseCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return ResultDto<CreateLicenseResponse>.Fail(
                "License request validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var subscription = await subscriptions.GetForLicenseIssuanceAsync(command.SubscriptionId, cancellationToken);
        if (subscription is null)
            return ResultDto<CreateLicenseResponse>.Fail("Subscription was not found.", failureKind: ResultFailureKind.NotFound);
        if (subscription.CurrentLicenseId is not null)
            return ResultDto<CreateLicenseResponse>.Fail("Subscription already has a current license.", failureKind: ResultFailureKind.Conflict);
        if (subscription.Status is SubscriptionStatusEnum.Canceled or SubscriptionStatusEnum.Expired)
            return ResultDto<CreateLicenseResponse>.Fail("A canceled or expired subscription cannot receive a license.", failureKind: ResultFailureKind.Conflict);

        var availableFeatureIds = subscription.Plan.PlanFeatures.Select(link => link.FeatureId).ToHashSet();
        var requestedFeatureIds = command.FeatureIds?.ToHashSet() ?? availableFeatureIds;
        if (requestedFeatureIds.Count != (command.FeatureIds?.Count ?? requestedFeatureIds.Count)
            || requestedFeatureIds.Any(featureId => !availableFeatureIds.Contains(featureId)))
            return ResultDto<CreateLicenseResponse>.Fail("One or more selected features are not available in the subscription plan.");

        var record = new CreateLicenseRecord(subscription.Id, command.IssuedByAdminId, licenseService.GenerateKey(),
            LicenseStatusEnum.PENDING, subscription.StartDate, subscription.EndDate, command.MaxActivations,
            subscription.CustomerId, subscription.ProductId, subscription.PlanId, FeatureIds: requestedFeatureIds);
        var license = licenses.Create(record, subscription);
        var signature = signer.Sign(license.GetSigningData());
        license.SetSignature(signature);

        var auditRecord = await auditLogService.ValidateLicenseIssuedAsync(command.IssuedByAdminId,
            command.IssuedByUserId, license.Id, cancellationToken);
        if (!auditRecord.IsSuccess || auditRecord.Data is null)
            return ResultDto<CreateLicenseResponse>.Fail(auditRecord.Message, auditRecord.Errors, auditRecord.FailureKind ?? ResultFailureKind.Validation);
        auditLogs.Create(auditRecord.Data, license);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ResultDto<CreateLicenseResponse>.Success("License created.",
            new CreateLicenseResponse(license.Id, license.Key.Value, signature.Value,
                subscription.ProductId, subscription.PlanId, license.StartDate, license.ExpirationDate));
    }
}
