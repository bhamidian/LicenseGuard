using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Records;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.UpdateLicense;

public sealed record UpdateLicenseCommand(
    Guid LicenseId,
    Guid AdminId,
    Guid UserId,
    string? Description,
    bool? AutoRenewalEnabled,
    AutoRenwalPlanEnum? AutoRenewalPlan,
    int? MaxActivations,
    IReadOnlyCollection<Guid>? FeatureIds,
    IReadOnlyCollection<LicenseLimitUpdateRecord>? Limits,
    string? Reason = null) : IRequest<ResultDto<UpdateLicenseResponse>>;

public sealed record UpdateLicenseResponse(
    Guid LicenseId,
    string Signature,
    string Description,
    bool AutoRenewalEnabled,
    AutoRenwalPlanEnum? AutoRenewalPlan,
    int MaxActivations,
    IReadOnlyCollection<Guid> EnabledFeatureIds,
    IReadOnlyCollection<LicenseLimitUpdateRecord> Limits,
    DateTime UpdatedAt);
