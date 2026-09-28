using LicenseGuard.Domain.Enums;

namespace LicenseGuard.Domain.Entities
{
    public class LicenseStatusHistory : BaseEntity
    {
        private LicenseStatusHistory() { }

        public LicenseStatusHistory(Guid licenseId, LicenseStatusEnum status, Guid? changedByUserId, string? reason = null, DateTime? changedAt = null)
        {
            if (licenseId == Guid.Empty)
                throw new Exceptions.DomainValidationException("License ID cannot be empty.");
            if (!Enum.IsDefined(status))
                throw new Exceptions.DomainValidationException("License status is invalid.");
            if (changedByUserId == Guid.Empty)
                throw new Exceptions.DomainValidationException("Changed-by user ID cannot be empty when supplied.");
            LicenseId = licenseId;
            Status = status;
            ChangedByUserId = changedByUserId;
            Reason = DomainText.Optional(reason, 1000, nameof(reason));
            ChangedAt = EnsureUtc(changedAt ?? DateTime.UtcNow);
        }

        public Guid LicenseId { get; private set; }
        public License License { get; private set; } = null!;
        public LicenseStatusEnum Status { get; private set; }
        public DateTime ChangedAt { get; private set; }
        public string? Reason { get; private set; }
        public Guid? ChangedByUserId { get; private set; }

        internal void SetLicense(License license)
        {
            ArgumentNullException.ThrowIfNull(license);
            if (license.Id != LicenseId)
                throw new Exceptions.DomainRuleViolationException("Status history belongs to another license.");
            License = license;
        }
    }
}
