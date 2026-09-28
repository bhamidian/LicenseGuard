namespace LicenseGuard.Domain.Entities
{
    public class Feature : BaseEntity
    {
        private Feature() { }

        public Feature(string code, string name, string description) => SetDetails(code, name, description);

        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public ICollection<PlanFeature> PlanFeatures { get; private set; } = new List<PlanFeature>();
        public ICollection<LicenseFeature> LicenseFeatures { get; private set; } = new List<LicenseFeature>();
        public ICollection<ProductFeature> ProductFeatures { get; private set; } = new List<ProductFeature>();


        public void SetDetails(string code, string name, string description)
        {
            Code = DomainText.Required(code, 100, nameof(code)).ToUpperInvariant();
            Name = DomainText.Required(name, 200, nameof(name));
            Description = DomainText.Optional(description, 2000, nameof(description));
            Touch();
        }

        public PlanFeature AddToPlan(Plan plan)
        {
            ArgumentNullException.ThrowIfNull(plan);
            return plan.AddFeature(this);
        }
    }
}
