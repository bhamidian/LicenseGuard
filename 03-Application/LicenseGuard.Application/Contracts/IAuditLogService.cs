using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Application.Features.License.Services;

public interface IAuditLogService
{
    Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseIssuedAsync(Guid adminId, Guid userId, Guid licenseId,
        CancellationToken cancellationToken);
    Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseActivatedAsync(Guid licenseId, Guid activationId,
        string ipAddress, CancellationToken cancellationToken);
    Task<ResultDto<CreateAuditLogRecord>> ValidateSubscriptionRenewedAsync(Guid subscriptionId, Guid userId,
        Guid? licenseId, decimal amount, DateTime oldExpirationDate, DateTime newExpirationDate,
        CancellationToken cancellationToken);
    Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseStatusChangedAsync(Guid adminId, Guid userId, Guid licenseId,
        string action, string oldStatus, string newStatus, string? reason, CancellationToken cancellationToken);
    Task<ResultDto<CreateAuditLogRecord>> ValidateActivationDeactivatedAsync(Guid adminId, Guid userId,
        Guid licenseId, Guid activationId, string? reason, CancellationToken cancellationToken);
    Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseExpiredAsync(Guid licenseId, string oldStatus,
        DateTime processedAt, CancellationToken cancellationToken);
    Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseUpdatedAsync(Guid adminId, Guid userId,
        Guid licenseId, string oldValues, string newValues, string? reason, CancellationToken cancellationToken);
}
