using FluentValidation;
using LicenseGuard.Application.Contracts;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Domain.ValueObjects;
using LicenseGuard.Infrstructure.SecurityService.Contracts;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.ValidateLicense;

public sealed class ValidateLicenseCommandHandler(
    IValidator<ValidateLicenseCommand> validator,
    ILicenseRepository licenses,
    IUnitOfWork unitOfWork,
    ILicenseSigner signer)
    : IRequestHandler<ValidateLicenseCommand, ResultDto<ValidateLicenseResponse>>
{
    public async Task<ResultDto<ValidateLicenseResponse>> Handle(
        ValidateLicenseCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<ValidateLicenseResponse>.Fail("License validation request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        var key = LicenseKey.Create(command.LicenseKey);
        var license = await licenses.GetForValidationAsync(new LicenseKeyLookupRecord(key.Value), cancellationToken);
        if (license?.Signature is null || !signer.Verify(license.GetSigningData(), license.Signature))
            return ResultDto<ValidateLicenseResponse>.Success("License is invalid.", new(false, null, null));

        var now = DateTime.UtcNow;
        var machineId = MachineId.Create(command.MachineId);
        var instanceId = InstanceId.Create(command.InstanceId);
        var activation = license.Activations.SingleOrDefault(item => item.Status == ActivationStatusEnum.Active
            && item.MachineId.Equals(machineId) && item.InstanceId.Equals(instanceId));

        if (!license.IsValidAt(now) || activation is null)
            return ResultDto<ValidateLicenseResponse>.Success("License is invalid.", new(false, null, null));

        activation.MarkValidated(now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultDto<ValidateLicenseResponse>.Success("License is valid.",
            new(true, license.GetSigningData(), license.Signature.Value));
    }
}
