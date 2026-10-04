using LicenseGuard.Domain.Dtos;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.DeactivateLicenseActivation;

public sealed record DeactivateLicenseActivationCommand(
    Guid LicenseId,
    Guid ActivationId,
    Guid AdminId,
    Guid UserId,
    string? Reason = null) : IRequest<ResultDto<DeactivateLicenseActivationResponse>>;

public sealed record DeactivateLicenseActivationResponse(Guid LicenseId, Guid ActivationId,
    DateTime DeactivatedAt);
