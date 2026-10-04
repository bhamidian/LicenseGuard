using System.Data;
using FluentValidation;
using LicenseGuard.Application.Features.License.Services;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Exceptions;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.ExpireLicenses;

public sealed class ExpireLicensesCommandHandler(
    IValidator<ExpireLicensesCommand> validator,
    ILicenseRepository licenses,
    IAuditLogRepository auditLogs,
    IAuditLogService auditLogService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ExpireLicensesCommand, ResultDto<int>>
{
    public async Task<ResultDto<int>> Handle(ExpireLicensesCommand command,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<int>.Fail("License expiration request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        var now = DateTime.UtcNow;
        var candidates = await licenses.GetExpiredLicenseIdsAsync(now, command.BatchSize, cancellationToken);
        var expiredCount = 0;

        foreach (var licenseId in candidates)
        {
            await using var transaction = await unitOfWork.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            var license = await licenses.GetForStatusChangeAsync(licenseId, cancellationToken);
            if (license is null || license.ExpirationDate > now || !CanExpire(license.LicenseStatus))
                continue;

            var oldStatus = license.LicenseStatus;
            var existingHistoryIds = license.StatusHistory.Select(history => history.Id).ToHashSet();
            try
            {
                license.Expire(now);
            }
            catch (DomainRuleViolationException)
            {
                continue;
            }

            foreach (var history in license.StatusHistory.Where(history => !existingHistoryIds.Contains(history.Id)))
                licenses.AddStatusHistory(history);

            var auditRecord = await auditLogService.ValidateLicenseExpiredAsync(
                license.Id, oldStatus.ToString(), now, cancellationToken);
            if (!auditRecord.IsSuccess || auditRecord.Data is null)
                throw new InvalidOperationException(auditRecord.Message);

            auditLogs.Create(auditRecord.Data, license);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            expiredCount++;
        }

        return ResultDto<int>.Success("Expired licenses processed.", expiredCount);
    }

    private static bool CanExpire(LicenseStatusEnum status) => status is
        LicenseStatusEnum.PENDING or LicenseStatusEnum.ACTIVE or LicenseStatusEnum.SUSPENDED;
}
