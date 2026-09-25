namespace LicenseGuard.Domain.Entities
{
    public class Plan : BaseEntity
    {
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public decimal Price { get; private set; }
        public Guid ProductId { get; private set; }
        public Product Product { get; private set; } = null!;
        public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
        public ICollection<PlanFeature> PlanFeatures { get; private set; } = new List<PlanFeature>();

    }
}
