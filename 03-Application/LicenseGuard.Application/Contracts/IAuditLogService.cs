using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Application.Features.License.Services;

public interface IAuditLogService
{
    Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseIssuedAsync(Guid adminId, Guid licenseId, CancellationToken cancellationToken);
}
