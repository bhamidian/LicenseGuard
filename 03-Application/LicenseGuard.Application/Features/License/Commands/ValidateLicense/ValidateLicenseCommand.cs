using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.ValidateLicense;

public sealed record ValidateLicenseCommand(
    string LicenseKey,
    string MachineId,
    string InstanceId) : IRequest<ResultDto<ValidateLicenseResponse>>;

public sealed record ValidateLicenseResponse(
    bool IsValid,
    LicenseSigningData? SigningData,
    string? Signature);
