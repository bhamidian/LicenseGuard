using System.Data;
using FluentValidation;
using LicenseGuard.Application.Features.License.Services;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Exceptions;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.ChangeLicenseStatus;

public sealed class ChangeLicenseStatusCommandHandler(
    IValidator<ChangeLicenseStatusCommand> validator,
    ILicenseRepository licenses,
    IAuditLogRepository auditLogs,
    IAuditLogService auditLogService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeLicenseStatusCommand, ResultDto<ChangeLicenseStatusResponse>>
{
    public async Task<ResultDto<ChangeLicenseStatusResponse>> Handle(
        ChangeLicenseStatusCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<ChangeLicenseStatusResponse>.Fail("License status request validation failed.",
                validation.Errors.Select(error => error.ErrorMessage));

        await using var transaction = await unitOfWork.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var license = await licenses.GetForStatusChangeAsync(command.LicenseId, cancellationToken);
        if (license is null)
            return ResultDto<ChangeLicenseStatusResponse>.Fail("License was not found.",
                failureKind: ResultFailureKind.NotFound);

        var oldStatus = license.LicenseStatus;
        var existingHistoryIds = license.StatusHistory.Select(history => history.Id).ToHashSet();
        try
        {
            switch (command.Action)
            {
                case LicenseStatusAction.Approve:
                    license.Approve(command.AdminId, command.Reason);
                    break;
                case LicenseStatusAction.Suspend:
                    license.Suspend(command.AdminId, command.Reason);
                    break;
                case LicenseStatusAction.Resume:
                    license.Resume(command.AdminId, command.Reason);
                    break;
                case LicenseStatusAction.Revoke:
                    license.Revoke(command.AdminId, command.Reason);
                    break;
                default:
                    return ResultDto<ChangeLicenseStatusResponse>.Fail("License status action is invalid.");
            }
        }
        catch (DomainRuleViolationException exception)
        {
            return ResultDto<ChangeLicenseStatusResponse>.Fail(exception.Message,
                failureKind: ResultFailureKind.Conflict);
        }
        catch (DomainValidationException exception)
        {
            return ResultDto<ChangeLicenseStatusResponse>.Fail(exception.Message);
        }

        var newStatus = license.LicenseStatus;
        foreach (var history in license.StatusHistory.Where(history => !existingHistoryIds.Contains(history.Id)))
            licenses.AddStatusHistory(history);
        if (oldStatus == newStatus)
        {
            await transaction.CommitAsync(cancellationToken);
            return ResultDto<ChangeLicenseStatusResponse>.Success("License status was already set.",
                new ChangeLicenseStatusResponse(license.Id, newStatus));
        }

        var auditRecord = await auditLogService.ValidateLicenseStatusChangedAsync(command.AdminId, command.UserId,
            license.Id, GetAuditAction(command.Action), oldStatus.ToString(), newStatus.ToString(),
            command.Reason, cancellationToken);
        if (!auditRecord.IsSuccess || auditRecord.Data is null)
            return ResultDto<ChangeLicenseStatusResponse>.Fail(auditRecord.Message, auditRecord.Errors,
                auditRecord.FailureKind ?? ResultFailureKind.Validation);

        auditLogs.Create(auditRecord.Data, license);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ResultDto<ChangeLicenseStatusResponse>.Success("License status updated.",
            new ChangeLicenseStatusResponse(license.Id, newStatus));
    }

    private static string GetAuditAction(LicenseStatusAction action) => action switch
    {
        LicenseStatusAction.Approve => "license.approved",
        LicenseStatusAction.Suspend => "license.suspended",
        LicenseStatusAction.Resume => "license.resumed",
        LicenseStatusAction.Revoke => "license.revoked",
        _ => "license.status_changed"
    };
}
