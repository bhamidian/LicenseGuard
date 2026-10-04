using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using MediatR;

namespace LicenseGuard.Application.Features.License.Queries.GetLicenseDetails;

public sealed record GetLicenseDetailsQuery(Guid LicenseId) : IRequest<ResultDto<LicenseDetailsResponse>>;

public sealed record LicenseDetailsResponse(
    Guid LicenseId,
    string LicenseKey,
    string Signature,
    Guid CustomerId,
    Guid ProductId,
    string? ProductName,
    Guid PlanId,
    string? PlanName,
    Guid IssuedByAdminId,
    LicenseStatusEnum Status,
    string PolicyVersion,
    DateTime CreatedAt,
    DateTime StartDate,
    DateTime ExpirationDate,
    int MaxActivations,
    int ActiveActivations,
    bool AutoRenewalEnabled,
    IReadOnlyCollection<LicenseFeatureResponse> Features,
    IReadOnlyCollection<LicenseLimitResponse> Limits,
    IReadOnlyCollection<LicenseActivationResponse> Activations,
    IReadOnlyCollection<LicenseStatusHistoryResponse> History,
    IReadOnlyCollection<LicenseAuditLogResponse> AuditLog);

public sealed record LicenseFeatureResponse(Guid FeatureId, string Code, bool IsEnabled);
public sealed record LicenseLimitResponse(string Code, decimal Value, string? Unit);
public sealed record LicenseActivationResponse(Guid ActivationId, string MachineId, string InstanceId,
    DateTime ActivatedAt, DateTime? LastValidatedTime, DateTime? DeactivatedAt, ActivationStatusEnum Status);
public sealed record LicenseStatusHistoryResponse(Guid HistoryId, LicenseStatusEnum Status,
    DateTime ChangedAt, Guid? ChangedByUserId, string? Reason);
public sealed record LicenseAuditLogResponse(Guid AuditLogId, string Action, DateTime CreatedAt,
    Guid? UserId, string? OldValues, string? NewValues, string? Metadata);
