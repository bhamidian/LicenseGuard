using LicenseGuard.Domain.Common.Entities;

namespace LicenseGuard.Domain.PlanAgg.Entities
{
    public class Plan : BaseEntity
    {
        private readonly List<Guid> _subscriptionIds = new List<Guid>();
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public decimal Price { get; private set; }
        public int DurationInDays { get; private set; }
        public int MaxActivationCount { get; private set; } 
        public double StorageLimitInMB { get; private set; }
        public int ApiRequestLimitPerDay { get; private set; }
        public IReadOnlyCollection<Guid> SubscriptionIds => _subscriptionIds;

    }
}