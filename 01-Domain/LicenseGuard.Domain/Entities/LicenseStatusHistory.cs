using LicenseGuard.Domain.Enums;

namespace LicenseGuard.Domain.Entities
{
    public class LicenseStatusHistory : BaseEntity
    {
        public Guid LicenseId { get; private set; }
        public License License { get; private set; } = null!;
        public LicenseStatusEnum Status { get; private set; }
        public DateTime ChangedAt { get; private set; }
        public string? Reason { get; private set; }
        public Guid? ChangedByUserId { get; private set; }
    }
}
