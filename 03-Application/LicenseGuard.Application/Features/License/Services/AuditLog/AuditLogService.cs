using FluentValidation;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;
using System.Globalization;

namespace LicenseGuard.Application.Features.License.Services;

public sealed class AuditLogService(IValidator<CreateAuditLogRecord> validator) : IAuditLogService
{
    public async Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseActivatedAsync(Guid licenseId,
        Guid activationId, string ipAddress, CancellationToken cancellationToken)
    {
        var record = new CreateAuditLogRecord("license.activation.created", "LicenseActivation",
            activationId.ToString(), true, LicenseId: licenseId, IpAddress: ipAddress,
            NewValues: "Active");
        var validation = await validator.ValidateAsync(record, cancellationToken);
        return validation.IsValid
            ? ResultDto<CreateAuditLogRecord>.Success("Activation audit record validated.", record)
            : ResultDto<CreateAuditLogRecord>.Fail("Activation audit record validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
    }

    public async Task<ResultDto<CreateAuditLogRecord>> ValidateSubscriptionRenewedAsync(Guid subscriptionId,
        Guid userId, Guid? licenseId, decimal amount, DateTime oldExpirationDate, DateTime newExpirationDate,
        CancellationToken cancellationToken)
    {
        var record = new CreateAuditLogRecord("subscription.renewed", "Subscription", subscriptionId.ToString(),
            true, UserId: userId, LicenseId: licenseId,
            OldValues: oldExpirationDate.ToString("O", CultureInfo.InvariantCulture),
            NewValues: newExpirationDate.ToString("O", CultureInfo.InvariantCulture),
            Metadata: $"Amount={amount.ToString(CultureInfo.InvariantCulture)}");
        var validation = await validator.ValidateAsync(record, cancellationToken);
        return validation.IsValid
            ? ResultDto<CreateAuditLogRecord>.Success("Renewal audit record validated.", record)
            : ResultDto<CreateAuditLogRecord>.Fail("Renewal audit record validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
    }

    public async Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseIssuedAsync(Guid adminId, Guid userId,
        Guid licenseId, CancellationToken cancellationToken)
    {
        var record = new CreateAuditLogRecord("license.issued", "License", licenseId.ToString(), true,
            UserId: userId, LicenseId: licenseId, Metadata: $"IssuedByAdminProfileId={adminId:D}");
        var validation = await validator.ValidateAsync(record, cancellationToken);
        return validation.IsValid
            ? ResultDto<CreateAuditLogRecord>.Success("Audit record validated.", record)
            : ResultDto<CreateAuditLogRecord>.Fail("Audit record validation failed.", validation.Errors.Select(error => error.ErrorMessage));
    }

    public async Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseStatusChangedAsync(Guid adminId, Guid userId,
        Guid licenseId, string action, string oldStatus, string newStatus, string? reason,
        CancellationToken cancellationToken)
    {
        var record = new CreateAuditLogRecord(action, "License", licenseId.ToString(), true,
            UserId: userId, LicenseId: licenseId, OldValues: oldStatus, NewValues: newStatus,
            Metadata: $"AdminProfileId={adminId:D}; Reason={reason}");
        var validation = await validator.ValidateAsync(record, cancellationToken);
        return validation.IsValid
            ? ResultDto<CreateAuditLogRecord>.Success("Audit record validated.", record)
            : ResultDto<CreateAuditLogRecord>.Fail("Audit record validation failed.", validation.Errors.Select(error => error.ErrorMessage));
    }

    public async Task<ResultDto<CreateAuditLogRecord>> ValidateActivationDeactivatedAsync(Guid adminId,
        Guid userId, Guid licenseId, Guid activationId, string? reason, CancellationToken cancellationToken)
    {
        var record = new CreateAuditLogRecord("license.activation.deactivated", "LicenseActivation",
            activationId.ToString(), true, UserId: userId, LicenseId: licenseId,
            OldValues: "Active", NewValues: "Deactivated",
            Metadata: $"AdminProfileId={adminId:D}; Reason={reason}");
        var validation = await validator.ValidateAsync(record, cancellationToken);
        return validation.IsValid
            ? ResultDto<CreateAuditLogRecord>.Success("Audit record validated.", record)
            : ResultDto<CreateAuditLogRecord>.Fail("Audit record validation failed.", validation.Errors.Select(error => error.ErrorMessage));
    }

    public async Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseExpiredAsync(Guid licenseId,
        string oldStatus, DateTime processedAt, CancellationToken cancellationToken)
    {
        var record = new CreateAuditLogRecord("license.expired", "License", licenseId.ToString(), true,
            LicenseId: licenseId, OldValues: oldStatus, NewValues: "EXPIRED",
            Metadata: $"ProcessedAt={processedAt:O}");
        var validation = await validator.ValidateAsync(record, cancellationToken);
        return validation.IsValid
            ? ResultDto<CreateAuditLogRecord>.Success("Expiration audit record validated.", record)
            : ResultDto<CreateAuditLogRecord>.Fail("Expiration audit record validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
    }

    public async Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseUpdatedAsync(Guid adminId, Guid userId,
        Guid licenseId, string oldValues, string newValues, string? reason, CancellationToken cancellationToken)
    {
        var record = new CreateAuditLogRecord("license.updated", "License", licenseId.ToString(), true,
            UserId: userId, LicenseId: licenseId, OldValues: oldValues, NewValues: newValues,
            Metadata: $"AdminProfileId={adminId:D}; Reason={reason}");
        var validation = await validator.ValidateAsync(record, cancellationToken);
        return validation.IsValid
            ? ResultDto<CreateAuditLogRecord>.Success("Audit record validated.", record)
            : ResultDto<CreateAuditLogRecord>.Fail("Audit record validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));
    }
}
