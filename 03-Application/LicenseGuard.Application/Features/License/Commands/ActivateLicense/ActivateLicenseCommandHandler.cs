using FluentValidation;
using LicenseGuard.Application.Contracts;
using LicenseGuard.Application.Features.License.Services;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Domain.ValueObjects;
using LicenseGuard.Infrstructure.SecurityService.Contracts;
using MediatR;
using System.Data;

namespace LicenseGuard.Application.Features.License.Commands.ActivateLicense;

public sealed class ActivateLicenseCommandHandler(
    IValidator<ActivateLicenseCommand> validator,
    ILicenseRepository licenses,
    ILicenseActivationRepository activations,
    IUnitOfWork unitOfWork,
    ILicenseSigner signer,
    IAuditLogRepository auditLogs,
    IAuditLogService auditLogService)
    : IRequestHandler<ActivateLicenseCommand, ResultDto<ActivateLicenseResponse>>
{
    public async Task<ResultDto<ActivateLicenseResponse>> Handle(
        ActivateLicenseCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<ActivateLicenseResponse>.Fail("License activation validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));

        await using var transaction = await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var key = LicenseKey.Create(command.LicenseKey);
        var license = await licenses.GetForActivationAsync(new LicenseKeyLookupRecord(key.Value), cancellationToken);
        if (license is null)
            return ResultDto<ActivateLicenseResponse>.Fail("License is invalid.", failureKind: ResultFailureKind.NotFound);

        if (license.Signature is null || !signer.Verify(license.GetSigningData(), license.Signature))
            return ResultDto<ActivateLicenseResponse>.Fail("License signature is invalid.", failureKind: ResultFailureKind.Conflict);

        var now = DateTime.UtcNow;
        if (!license.IsValidAt(now))
            return ResultDto<ActivateLicenseResponse>.Fail("License is not currently valid.", failureKind: ResultFailureKind.Conflict);

        var maxActivations = (int)(license.Limits.SingleOrDefault(limit => limit.Code == "max_activations")?.Value ?? 1);
        if (license.Activations.Count(activation => activation.Status == ActivationStatusEnum.Active) >= maxActivations)
            return ResultDto<ActivateLicenseResponse>.Fail("License activation limit has been reached.", failureKind: ResultFailureKind.Conflict);

        var machineId = MachineId.Create(command.MachineId);
        var instanceId = InstanceId.Create(command.InstanceId);
        if (license.Activations.Any(activation => activation.Status == ActivationStatusEnum.Active &&
            (activation.MachineId.Equals(machineId) || activation.InstanceId.Equals(instanceId))))
            return ResultDto<ActivateLicenseResponse>.Fail("This machine or instance is already activated.", failureKind: ResultFailureKind.Conflict);

        var activation = activations.Create(new CreateLicenseActivationRecord(
            license.Id, machineId, instanceId, command.IpAddress, now));
        license.Activate(activation, now);

        var auditRecord = await auditLogService.ValidateLicenseActivatedAsync(
            license.Id, activation.Id, command.IpAddress, cancellationToken);
        if (!auditRecord.IsSuccess || auditRecord.Data is null)
            return ResultDto<ActivateLicenseResponse>.Fail(auditRecord.Message, auditRecord.Errors,
                auditRecord.FailureKind ?? ResultFailureKind.Validation);
        auditLogs.Create(auditRecord.Data, license);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ResultDto<ActivateLicenseResponse>.Success("License activated.",
            new ActivateLicenseResponse(activation.Id, license.GetSigningData(), license.Signature.Value));
    }
}
