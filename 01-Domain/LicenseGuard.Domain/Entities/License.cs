

using LicenseGuard.Domain.Enums;

namespace LicenseGuard.Domain.Entities
{
    public class License : BaseEntity
    {
        private License()
        {

        }
        public License(Guid subscrptionId, Guid adminId,LicenseStatusEnum licenseStatus,DateTime startDate, DateTime endDate)
        {
            CreatedBy = adminId;
            LicenseStatus = licenseStatus;
            SubscriptionId = subscrptionId;
            StartDate = startDate;
            ExpirationDate = endDate;

        }
        public DateTime StartDate { get; private set; }
        public Guid SubscriptionId { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public LicenseKey Key { get; private set; } = null!;
        public LicenseStatusEnum LicenseStatus { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public Subscription Subscription { get; private set; } = null!;
        public LicenseSignature Signature { get; private set; } = null!;
        public AutoRenewal AutoRenewal { get; private set; } = new AutoRenewal(false);
        public ICollection<AuditLog> AuditLogs { get; private set; } = new List<AuditLog>();
        public ICollection<LicenseLimit> Limits { get; private set; } = new List<LicenseLimit>();
        public ICollection<LicenseFeature> LicenseFeatures { get; private set; } = new List<LicenseFeature>();
        public ICollection<LicenseActivation> Activations { get; private set; } = new List<LicenseActivation>();
        public ICollection<LicenseStatusHistory> StatusHistory { get; private set; } = new List<LicenseStatusHistory>();

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
