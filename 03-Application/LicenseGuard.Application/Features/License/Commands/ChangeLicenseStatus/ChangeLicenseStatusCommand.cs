using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.ChangeLicenseStatus;

public enum LicenseStatusAction
{
    Approve,
    Suspend,
    Resume,
    Revoke
}

public sealed record ChangeLicenseStatusCommand(
    Guid LicenseId,
    Guid AdminId,
    Guid UserId,
    LicenseStatusAction Action,
    string? Reason = null) : IRequest<ResultDto<ChangeLicenseStatusResponse>>;

public sealed record ChangeLicenseStatusResponse(Guid LicenseId, LicenseStatusEnum Status);
