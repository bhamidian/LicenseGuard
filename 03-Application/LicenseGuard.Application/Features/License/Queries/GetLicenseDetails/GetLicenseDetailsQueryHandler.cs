using FluentValidation;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.License.Queries.GetLicenseDetails;

public sealed class GetLicenseDetailsQueryHandler(
    IValidator<GetLicenseDetailsQuery> validator,
    ILicenseRepository licenses)
    : IRequestHandler<GetLicenseDetailsQuery, ResultDto<LicenseDetailsResponse>>
{
    public async Task<ResultDto<LicenseDetailsResponse>> Handle(
        GetLicenseDetailsQuery query, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<LicenseDetailsResponse>.Fail("License lookup request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        var license = await licenses.GetDetailsAsync(query.LicenseId, cancellationToken);
        if (license is null)
            return ResultDto<LicenseDetailsResponse>.Fail("License was not found.",
                failureKind: ResultFailureKind.NotFound);

        return ResultDto<LicenseDetailsResponse>.Success("License retrieved.", Map(license));
    }

    private static LicenseDetailsResponse Map(LicenseGuard.Domain.Entities.License license) => new(
        license.Id,
        license.Key.Value,
        license.Signature.Value,
        license.CustomerId,
        license.ProductId,
        license.Subscription?.Product?.Name,
        license.PlanId,
        license.Subscription?.Plan?.Name,
        license.AdminId,
        license.LicenseStatus,
        license.PolicyVersion,
        license.CreatedAt,
        license.StartDate,
        license.ExpirationDate,
        (int)(license.Limits.SingleOrDefault(limit => limit.Code == "max_activations")?.Value ?? 1),
        license.Activations.Count(activation => activation.Status == Domain.Enums.ActivationStatusEnum.Active),
        license.AutoRenewal.IsEnabled,
        license.LicenseFeatures.OrderBy(feature => feature.FeatureCodeSnapshot)
            .Select(feature => new LicenseFeatureResponse(feature.FeatureId, feature.FeatureCodeSnapshot, feature.IsEnabled)).ToArray(),
        license.Limits.OrderBy(limit => limit.Code)
            .Select(limit => new LicenseLimitResponse(limit.Code, limit.Value, limit.Unit)).ToArray(),
        license.Activations.OrderByDescending(activation => activation.ActivatedAt)
            .Select(activation => new LicenseActivationResponse(activation.Id, activation.MachineId.Value,
                activation.InstanceId.Value, activation.ActivatedAt, activation.LastValidatedTime,
                activation.DeactivatedAt, activation.Status)).ToArray(),
        license.StatusHistory.OrderByDescending(history => history.ChangedAt)
            .Select(history => new LicenseStatusHistoryResponse(history.Id, history.Status, history.ChangedAt,
                history.ChangedByUserId, history.Reason)).ToArray(),
        license.AuditLogs.OrderByDescending(log => log.CreatedAt)
            .Select(log => new LicenseAuditLogResponse(log.Id, log.Action, log.CreatedAt, log.UserId,
                log.OldValues, log.NewValues, log.Metadata)).ToArray());
}
