using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Exceptions;
using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Domain.Entities
{
    public class LicenseActivation : BaseEntity
    {
        private LicenseActivation() { }

        public LicenseActivation(Guid licenseId, MachineId machineId, InstanceId instanceId, string ipAddress, DateTime? activatedAt = null)
        {
            if (licenseId == Guid.Empty) throw new DomainValidationException("License ID cannot be empty.");
            ArgumentNullException.ThrowIfNull(machineId);
            ArgumentNullException.ThrowIfNull(instanceId);
            LicenseId = licenseId;
            MachineId = machineId;
            InstanceId = instanceId;
            IpAddress = DomainText.Required(ipAddress, 64, nameof(ipAddress));
            ActivatedAt = EnsureUtc(activatedAt ?? DateTime.UtcNow);
            Status = ActivationStatusEnum.Active;
        }

        public Guid LicenseId { get; private set; }
        public License License { get; private set; } = null!;
        public MachineId MachineId { get; private set; } = null!;
        public InstanceId InstanceId { get; private set; } = null!;
        public DateTime ActivatedAt { get; private set; }
        public string IpAddress { get; private set; } = null!;
        public DateTime? DeactivatedAt { get; private set; }
        public DateTime? LastValidatedTime { get; private set; }
        public ActivationStatusEnum Status { get; private set; }

        public void MarkValidated(DateTime? at = null)
        {
            EnsureActive();
            var instant = EnsureUtc(at ?? DateTime.UtcNow);
            if (instant < ActivatedAt)
                throw new DomainValidationException("Validation time cannot precede activation time.");
            LastValidatedTime = instant;
            Touch(LastValidatedTime);
        }

        public void Deactivate(DateTime? at = null)
        {
            EnsureActive();
            var instant = EnsureUtc(at ?? DateTime.UtcNow);
            if (instant < ActivatedAt)
                throw new DomainValidationException("Deactivation time cannot precede activation time.");
            DeactivatedAt = instant;
            Status = ActivationStatusEnum.Deactivated;
            Touch(DeactivatedAt);
        }

        internal void AttachToLicense(License license)
        {
            ArgumentNullException.ThrowIfNull(license);
            if (license.Id != LicenseId)
                throw new DomainRuleViolationException("Activation belongs to another license.");
            License = license;
        }

        private void EnsureActive()
        {
            if (Status != ActivationStatusEnum.Active)
                throw new DomainRuleViolationException("A deactivated activation cannot be changed.");
        }
    }
}
