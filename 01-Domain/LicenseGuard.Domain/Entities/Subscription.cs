using LicenseGuard.Domain.Enums;

namespace LicenseGuard.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public Guid CustomerId { get; private set; }
        public Customer Customer { get; private set; } = null!;
        public Guid ProductId { get; private set; }
        public Product Product { get; private set; } = null!;
        public Guid PlanId { get; private set; }
        public Plan Plan { get; private set; } = null!;
        public Guid? CurrentLicenseId { get; private set; }
        public License? CurrentLicense { get; private set; }
        public ICollection<License> Licenses { get; private set; } = new List<License>();
        public ICollection<SubscriptionRenewal> Renewals { get; private set; } = new List<SubscriptionRenewal>();
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public SubscriptionStatusEnum Status { get; private set; }
        public string? MetaData { get; private set; }

        public void SetCurrentLicense(License license)
        {
            ArgumentNullException.ThrowIfNull(license);

            if (Id == Guid.Empty || license.Id == Guid.Empty || license.SubscriptionId != Id)
                throw new InvalidOperationException("The current license must belong to this subscription.");

            CurrentLicense = license;
            CurrentLicenseId = license.Id;
        }

        public void ClearCurrentLicense()
        {
            CurrentLicense = null;
            CurrentLicenseId = null;
        }
    }
}
