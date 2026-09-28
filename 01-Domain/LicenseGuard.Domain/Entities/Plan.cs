namespace LicenseGuard.Domain.Entities
{
    public class Plan : BaseEntity
    {
        private Plan() { }

        public Plan(Guid productId, string name, string description, decimal price)
        {
            if (productId == Guid.Empty)
                throw new Exceptions.DomainValidationException("Product ID cannot be empty.");
            ProductId = productId;
            Product = null!;
            SetDetails(name, description, price);
        }

        public Plan(Product product, string name, string description, decimal price)
        {
            ArgumentNullException.ThrowIfNull(product);
            if (product.Id == Guid.Empty)
                throw new Exceptions.DomainValidationException("Product ID cannot be empty.");
            ProductId = product.Id;
            Product = product;
            SetDetails(name, description, price);
        }

        public decimal Price { get; private set; }
        public Guid ProductId { get; private set; }
        public string Name { get; private set; } = null!;
        public Product Product { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public ICollection<PlanFeature> PlanFeatures { get; private set; } = new List<PlanFeature>();
        public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();

        public void SetDetails(string name, string description, decimal price)
        {
            if (price < 0)
                throw new Exceptions.DomainValidationException("Plan price cannot be negative.");
            Name = DomainText.Required(name, 200, nameof(name));
            Description = DomainText.Optional(description, 2000, nameof(description));
            Price = price;
            Touch();
        }

        public PlanFeature AddFeature(Feature feature)
        {
            ArgumentNullException.ThrowIfNull(feature);
            if (!feature.ProductFeatures.Any(link => link.ProductId == ProductId))
                throw new Exceptions.DomainRuleViolationException("Feature is not available for this plan's product.");
            if (PlanFeatures.Any(link => link.FeatureId == feature.Id))
                throw new Exceptions.DomainRuleViolationException("Feature is already attached to this plan.");
            var link = new PlanFeature(this, feature);
            PlanFeatures.Add(link);
            feature.PlanFeatures.Add(link);
            return link;
        }

    }
}
