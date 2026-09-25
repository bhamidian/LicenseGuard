namespace LicenseGuard.Domain.Entities
{
    public class PlanFeature : BaseEntity
    {
        public Guid PlanId { get; private set; }
        public Plan Plan { get; private set; } = null!;
        public Guid FeatureId { get; private set; }
        public Feature Feature { get; private set; } = null!;
    }
}
