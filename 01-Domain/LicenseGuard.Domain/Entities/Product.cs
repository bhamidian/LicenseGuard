namespace LicenseGuard.Domain.Entities
{
    public class Product : BaseEntity
    {
        private Product() { }

        public Product(string code, string name, string description) => SetDetails(code, name, description);

        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public ICollection<Plan> Plans { get; private set; } = new List<Plan>();
        public ICollection<ProductFeature> ProductFeatures { get; private set; } = new List<ProductFeature>();

        public void SetDetails(string code, string name, string description)
        {
            Code = DomainText.Required(code, 64, nameof(code)).ToUpperInvariant();
            Name = DomainText.Required(name, 200, nameof(name));
            Description = DomainText.Optional(description, 2000, nameof(description));
            Touch();
        }

        public Plan AddPlan(string name, string description, decimal price)
        {
            if (Plans.Any(plan => string.Equals(plan.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new Exceptions.DomainRuleViolationException("A plan with this name already exists for the product.");
            var plan = new Plan(this, name, description, price);
            Plans.Add(plan);
            return plan;
        }

        public ProductFeature AddFeature(Feature feature)
        {
            ArgumentNullException.ThrowIfNull(feature);
            if (ProductFeatures.Any(link => link.FeatureId == feature.Id))
                throw new Exceptions.DomainRuleViolationException("Feature is already attached to this product.");
            var link = new ProductFeature(this, feature);
            ProductFeatures.Add(link);
            feature.ProductFeatures.Add(link);
            return link;
        }

    }
}
