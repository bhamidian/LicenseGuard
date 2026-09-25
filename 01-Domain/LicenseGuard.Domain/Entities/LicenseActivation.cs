using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Domain.Entities
{
    public class LicenseActivation : BaseEntity
    {
        public Guid LicenseId { get; private set; }
        public License License { get; private set; } = null!;
        public MachineId MachineId { get; private set; }
        public InstanceId InstanceId { get; private set; } 
        public DateTime ActivatedAt { get; private set; }
        public string IpAddress { get; private set; } = null!;
        public DateTime? DeactivatedAt { get; private set; }
        public DateTime? LastValidatedTime { get; private set; }
        public ActivationStatusEnum Status { get; private set; }
    }
}
