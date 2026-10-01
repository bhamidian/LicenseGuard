namespace LicenseGuard.Domain.Records;

public sealed record CreateAuditLogRecord(
    string Action,
    string EntityType,
    string? EntityId,
    bool IsSuccess,
    Guid? UserId = null,
    Guid? LicenseId = null,
    Guid? CorrelationId = null,
    string? IpAddress = null,
    string? UserAgent = null,
    string? OldValues = null,
    string? NewValues = null,
    string? Metadata = null,
    DateTime? CreatedAt = null);
