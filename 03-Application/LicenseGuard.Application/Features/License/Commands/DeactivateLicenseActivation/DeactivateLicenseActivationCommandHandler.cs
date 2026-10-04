using System.Data;
using FluentValidation;
using LicenseGuard.Application.Features.License.Services;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Exceptions;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.DeactivateLicenseActivation;

public sealed class DeactivateLicenseActivationCommandHandler(
    IValidator<DeactivateLicenseActivationCommand> validator,
    ILicenseRepository licenses,
    IAuditLogRepository auditLogs,
    IAuditLogService auditLogService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateLicenseActivationCommand, ResultDto<DeactivateLicenseActivationResponse>>
{
    public async Task<ResultDto<DeactivateLicenseActivationResponse>> Handle(
        DeactivateLicenseActivationCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<DeactivateLicenseActivationResponse>.Fail("Activation deactivation request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        await using var transaction = await unitOfWork.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var license = await licenses.GetForActivationDeactivationAsync(command.LicenseId, cancellationToken);
        if (license is null)
            return ResultDto<DeactivateLicenseActivationResponse>.Fail("License was not found.",
                failureKind: ResultFailureKind.NotFound);

        var activation = license.Activations.SingleOrDefault(item => item.Id == command.ActivationId);
        if (activation is null)
            return ResultDto<DeactivateLicenseActivationResponse>.Fail("Activation was not found for this license.",
                failureKind: ResultFailureKind.NotFound);

        try
        {
            activation.Deactivate();
        }
        catch (DomainRuleViolationException exception)
        {
            return ResultDto<DeactivateLicenseActivationResponse>.Fail(exception.Message,
                failureKind: ResultFailureKind.Conflict);
        }
        catch (DomainValidationException exception)
        {
            return ResultDto<DeactivateLicenseActivationResponse>.Fail(exception.Message);
        }

        var auditRecord = await auditLogService.ValidateActivationDeactivatedAsync(command.AdminId,
            command.UserId, license.Id, activation.Id, command.Reason, cancellationToken);
        if (!auditRecord.IsSuccess || auditRecord.Data is null)
            return ResultDto<DeactivateLicenseActivationResponse>.Fail(auditRecord.Message, auditRecord.Errors,
                auditRecord.FailureKind ?? ResultFailureKind.Validation);

        auditLogs.Create(auditRecord.Data, license);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ResultDto<DeactivateLicenseActivationResponse>.Success("Activation deactivated.",
            new DeactivateLicenseActivationResponse(license.Id, activation.Id,
                activation.DeactivatedAt!.Value));
    }
}
