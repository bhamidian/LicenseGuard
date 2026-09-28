
using LicenseGuard.Domain.Exceptions;

namespace LicenseGuard.Domain.Entities
{
    public class AuditLog
    {
        private AuditLog() { }

        public AuditLog(string action, string entityType, string? entityId, bool isSuccess, Guid? userId = null,
            Guid? licenseId = null, Guid? correlationId = null, string? ipAddress = null,
            string? userAgent = null, string? oldValues = null, string? newValues = null, string? metadata = null,
            DateTime? createdAt = null)
        {
            if (string.IsNullOrWhiteSpace(action)) throw new DomainValidationException("Audit action is required.");
            if (string.IsNullOrWhiteSpace(entityType)) throw new DomainValidationException("Audit entity type is required.");
            if (userId == Guid.Empty || licenseId == Guid.Empty || correlationId == Guid.Empty)
                throw new DomainValidationException("Optional audit identifiers cannot be empty GUIDs.");
            Id = Guid.NewGuid();
            Action = DomainText.Required(action, 200, nameof(action));
            EntityType = DomainText.Required(entityType, 200, nameof(entityType));
            EntityId = DomainText.Optional(entityId, 200, nameof(entityId));
            IsSuccess = isSuccess;
            UserId = userId;
            LicenseId = licenseId;
            CorrelationId = correlationId;
            IpAddress = DomainText.Optional(ipAddress, 64, nameof(ipAddress));
            UserAgent = DomainText.Optional(userAgent, 512, nameof(userAgent));
            OldValues = oldValues;
            NewValues = newValues;
            Metadata = metadata;
            var timestamp = createdAt ?? DateTime.UtcNow;
            CreatedAt = timestamp.Kind == DateTimeKind.Local ? timestamp.ToUniversalTime() : DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
        }

        public Guid Id { get; private set; }
        public Guid? UserId { get; private set; }
        public bool IsSuccess { get; private set; }
        public Guid? LicenseId { get; private set; }
        public License? License { get; private set; }
        public Guid? CorrelationId { get; private set; }
        public string Action { get; private set; } = null!;
        public string? EntityId { get; private set; } = null!;
        public string? Metadata { get; private set; } = null!;
        public string EntityType { get; private set; } = null!;
        public string? IpAddress { get; private set; } = null!;
        public string? NewValues { get; private set; } = null!;
        public string? OldValues { get; private set; } = null!;
        public string? UserAgent { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        internal void AttachToLicense(License license)
        {
            ArgumentNullException.ThrowIfNull(license);
            if (LicenseId is not null && LicenseId != license.Id)
                throw new DomainRuleViolationException("Audit log belongs to another license.");
            LicenseId = license.Id;
            License = license;
        }
    }
}
