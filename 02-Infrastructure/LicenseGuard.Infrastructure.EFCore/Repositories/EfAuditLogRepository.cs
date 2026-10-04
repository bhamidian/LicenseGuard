using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.EFCore.Persistence;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfAuditLogRepository(ApplicationDbContext dbContext) : IAuditLogRepository
{
    public AuditLog Create(CreateAuditLogRecord record)
    {
        var auditLog = new AuditLog(record.Action, record.EntityType, record.EntityId, record.IsSuccess,
            record.UserId, record.LicenseId, record.CorrelationId, record.IpAddress, record.UserAgent,
            record.OldValues, record.NewValues, record.Metadata, record.CreatedAt);
        dbContext.LicenseAuditLogs.Add(auditLog);
        return auditLog;
    }

    public AuditLog Create(CreateAuditLogRecord record, License license)
    {
        var auditLog = Create(record);
        license.AddAuditLog(auditLog);
        return auditLog;
    }
}
