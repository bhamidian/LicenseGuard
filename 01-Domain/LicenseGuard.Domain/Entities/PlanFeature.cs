namespace LicenseGuard.Domain.Entities
{
    public class PlanFeature : BaseEntity
    {
        private PlanFeature() { }

        public PlanFeature(Plan plan, Feature feature)
        {
            ArgumentNullException.ThrowIfNull(plan);
            ArgumentNullException.ThrowIfNull(feature);
            if (!feature.ProductFeatures.Any(link => link.ProductId == plan.ProductId))
                throw new Exceptions.DomainRuleViolationException("Feature is not available for the plan's product.");
            PlanId = plan.Id;
            FeatureId = feature.Id;
            Plan = plan;
            Feature = feature;
        }

        public Guid PlanId { get; private set; }
        public Guid FeatureId { get; private set; }
        public Plan Plan { get; private set; } = null!;
        public Feature Feature { get; private set; } = null!;
    }
}
