using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.ActivateLicense;

public sealed record ActivateLicenseCommand(
    string LicenseKey,
    string MachineId,
    string InstanceId,
    string IpAddress) : IRequest<ResultDto<ActivateLicenseResponse>>;

public sealed record ActivateLicenseResponse(
    Guid ActivationId,
    LicenseSigningData SigningData,
    string Signature);
