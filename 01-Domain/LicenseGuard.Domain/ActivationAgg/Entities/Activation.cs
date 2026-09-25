using LicenseGuard.Domain.ActivationAgg.Enums;
using LicenseGuard.Domain.ActivationAgg.ValueObjects;
using LicenseGuard.Domain.Common.Entities;

namespace LicenseGuard.Domain.ActivationAgg.Entities
{
    public class Activation : BaseEntity
    {
        public Guid LicenseId { get; private set; }
        public MachineId MachineId { get; private set; }
        public InstanceId InstanceId { get; private set; } 
        public DateTime ActivatedAt { get; private set; }
        public string IpAddress { get; private set; } = null!;
        public DateTime? DeactivatedAt { get; private set; }
        public DateTime? LastValidatedTime { get; private set; }
        public ActivationStatusEnum Status { get; private set; }

    }
}