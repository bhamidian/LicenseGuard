using System.Data;
using System.Text.Json;
using FluentValidation;
using LicenseGuard.Application.Features.License.Services;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Exceptions;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrstructure.SecurityService.Contracts;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.UpdateLicense;

public sealed class UpdateLicenseCommandHandler(
    IValidator<UpdateLicenseCommand> validator,
    ILicenseRepository licenses,
    IAuditLogRepository auditLogs,
    IAuditLogService auditLogService,
    IUnitOfWork unitOfWork,
    ILicenseSigner signer)
    : IRequestHandler<UpdateLicenseCommand, ResultDto<UpdateLicenseResponse>>
{
    public async Task<ResultDto<UpdateLicenseResponse>> Handle(
        UpdateLicenseCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<UpdateLicenseResponse>.Fail("License update request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        await using var transaction = await unitOfWork.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var license = await licenses.GetForUpdateAsync(command.LicenseId, cancellationToken);
        if (license is null)
            return ResultDto<UpdateLicenseResponse>.Fail("License was not found.",
                failureKind: ResultFailureKind.NotFound);
        if (license.LicenseStatus is LicenseStatusEnum.REVOKED or LicenseStatusEnum.EXPIRED)
            return ResultDto<UpdateLicenseResponse>.Fail("A revoked or expired license cannot be edited.",
                failureKind: ResultFailureKind.Conflict);

        IReadOnlyCollection<Feature>? selectedFeatures = null;
        if (command.FeatureIds is not null)
        {
            var available = await licenses.GetAvailableFeaturesForPlanAsync(license.PlanId, cancellationToken);
            var byId = available.ToDictionary(feature => feature.Id);
            if (command.FeatureIds.Any(featureId => !byId.ContainsKey(featureId)))
                return ResultDto<UpdateLicenseResponse>.Fail(
                    "One or more selected features are not available in the license plan.");
            selectedFeatures = command.FeatureIds.Select(featureId => byId[featureId]).ToArray();
        }

        var oldValues = SerializeSettings(license);
        try
        {
            if (command.Description is not null)
                license.SetDescription(command.Description);
            license.UpdateAutoRenewal(command.AutoRenewalEnabled, command.AutoRenewalPlan);
            license.UpdateSignedEntitlements(selectedFeatures, command.MaxActivations, command.Limits);
        }
        catch (DomainRuleViolationException exception)
        {
            return ResultDto<UpdateLicenseResponse>.Fail(exception.Message,
                failureKind: ResultFailureKind.Conflict);
        }
        catch (DomainValidationException exception)
        {
            return ResultDto<UpdateLicenseResponse>.Fail(exception.Message);
        }

        var signature = signer.Sign(license.GetSigningData());
        license.SetSignature(signature);

        var auditRecord = await auditLogService.ValidateLicenseUpdatedAsync(command.AdminId, command.UserId,
            license.Id, oldValues, SerializeSettings(license), command.Reason, cancellationToken);
        if (!auditRecord.IsSuccess || auditRecord.Data is null)
            return ResultDto<UpdateLicenseResponse>.Fail(auditRecord.Message, auditRecord.Errors,
                auditRecord.FailureKind ?? ResultFailureKind.Validation);

        auditLogs.Create(auditRecord.Data, license);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var maxActivations = (int)(license.Limits
            .SingleOrDefault(limit => limit.Code == "max_activations")?.Value ?? 1);
        var response = new UpdateLicenseResponse(license.Id, license.Signature.Value,
            license.Description, license.AutoRenewal.IsEnabled, license.AutoRenewal.Plan,
            maxActivations,
            license.LicenseFeatures.Where(feature => feature.IsEnabled).Select(feature => feature.FeatureId)
                .Order().ToArray(),
            license.Limits.Where(limit => limit.Code != "max_activations")
                .OrderBy(limit => limit.Code)
                .Select(limit => new LicenseLimitUpdateRecord(limit.Code, limit.Value, limit.Unit)).ToArray(),
            license.UpdatedAt ?? DateTime.UtcNow);

        return ResultDto<UpdateLicenseResponse>.Success("License updated and re-signed.", response);
    }

    private static string SerializeSettings(LicenseGuard.Domain.Entities.License license) =>
        JsonSerializer.Serialize(new
        {
            license.Description,
            AutoRenewalEnabled = license.AutoRenewal.IsEnabled,
            AutoRenewalPlan = license.AutoRenewal.Plan?.ToString(),
            MaxActivations = (int)(license.Limits
                .SingleOrDefault(limit => limit.Code == "max_activations")?.Value ?? 1),
            EnabledFeatureIds = license.LicenseFeatures.Where(feature => feature.IsEnabled)
                .Select(feature => feature.FeatureId).Order().ToArray(),
            Limits = license.Limits.Where(limit => limit.Code != "max_activations")
                .OrderBy(limit => limit.Code)
                .Select(limit => new { limit.Code, limit.Value, limit.Unit }).ToArray()
        });
}
