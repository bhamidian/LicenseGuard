using LicenseGuard.Domain.Common.Entities;
using LicenseGuard.Domain.LicenseAgg.Enums;
using LicenseGuard.Domain.LicenseAgg.Records;
using LicenseGuard.Domain.LicenseAgg.ValueObjects;

namespace LicenseGuard.Domain.LicenseAgg.Entities
{
    public class License : BaseEntity
    {
        private readonly List<Guid> _customerIds = new List<Guid>();
        public LicenseStatusEnum LicenseStatus { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        public DateTime StartDate { get; private set; }
        public LicenseKey Key { get; private set; } = null!;
        public LicenseSignature Signature { get; private set; } = null!;
        public string Description { get; private set; } = string.Empty;
        public Guid AdminId { get; private set; }
        public IReadOnlyCollection<Guid> CustomerIds => _customerIds;
        public Guid ProductId { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid PlanId { get; private set; }
        public AutoRenewal AutoRenewal { get; private set; } = new AutoRenewal(false);

        public LicenseSigningData GetSigningData()
        {
            return new LicenseSigningData(
                Key.Value,
                ProductId,
                PlanId,
                StartDate,
                ExpirationDate
            );
        }
        
        
    }
}