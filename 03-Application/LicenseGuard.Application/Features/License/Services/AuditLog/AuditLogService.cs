using FluentValidation;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Application.Features.License.Services;

public sealed class AuditLogService(IValidator<CreateAuditLogRecord> validator) : IAuditLogService
{
    public async Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseIssuedAsync(Guid adminId, Guid licenseId, CancellationToken cancellationToken)
    {
        var record = new CreateAuditLogRecord("license.issued", "License", licenseId.ToString(), true,
            UserId: adminId, LicenseId: licenseId);
        var validation = await validator.ValidateAsync(record, cancellationToken);
        return validation.IsValid
            ? ResultDto<CreateAuditLogRecord>.Success("Audit record validated.", record)
            : ResultDto<CreateAuditLogRecord>.Fail("Audit record validation failed.", validation.Errors.Select(error => error.ErrorMessage));
    }
}
