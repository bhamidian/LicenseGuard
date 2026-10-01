using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Domain.Repositories;

public interface IAuditLogRepository
{
    AuditLog Create(CreateAuditLogRecord record, License license);
}
