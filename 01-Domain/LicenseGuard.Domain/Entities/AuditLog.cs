
namespace LicenseGuard.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; private set; }
        public Guid? LicenseId { get; private set; }
        public License? License { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public Guid? UserId { get; private set; }
        public string Action { get; private set; } = null!;
        public string EntityType { get; private set; } = null!;
        public string? EntityId { get; private set; } = null!;
        public bool IsSuccess { get; private set; }
        public string? IpAddress { get; private set; } = null!;
        public string? UserAgent { get; private set; } = null!;
        public Guid? CorrelationId { get; private set; }
        public string? NewValues { get; private set; } = null!;
        public string? OldValues { get; private set; } = null!;
        public string? Metadata { get; private set; } = null!;
    }
}
