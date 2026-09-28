using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Domain.Entities
{
    public class License : BaseEntity
    {
        public Guid SubscriptionId { get; private set; }
        public Subscription Subscription { get; private set; } = null!;
        public ICollection<LicenseActivation> Activations { get; private set; } = new List<LicenseActivation>();
        public ICollection<LicenseStatusHistory> StatusHistory { get; private set; } = new List<LicenseStatusHistory>();
        public ICollection<LicenseFeature> LicenseFeatures { get; private set; } = new List<LicenseFeature>();
        public ICollection<LicenseLimit> Limits { get; private set; } = new List<LicenseLimit>();
        public ICollection<AuditLog> AuditLogs { get; private set; } = new List<AuditLog>();
        public LicenseStatusEnum LicenseStatus { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public DateTime StartDate { get; private set; }
        public LicenseKey Key { get; private set; } = null!;
        public LicenseSignature Signature { get; private set; } = null!;
        public string Description { get; private set; } = string.Empty;
        public Guid AdminId { get; private set; }
        public AutoRenewal AutoRenewal { get; private set; } = new AutoRenewal(false);

        public LicenseSigningData GetSigningData()
        {
            return new LicenseSigningData(
                Key.Value,
                Subscription.ProductId,
                Subscription.PlanId,
                StartDate,
                ExpirationDate
            );
        }


    }
}
