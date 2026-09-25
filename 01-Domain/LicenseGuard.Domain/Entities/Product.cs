namespace LicenseGuard.Domain.Entities
{
    public class Product : BaseEntity
    {
        public ICollection<ProductFeature> ProductFeatures { get; private set; } = new List<ProductFeature>();
        public ICollection<Plan> Plans { get; private set; } = new List<Plan>();
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string Code { get; private set; } = null!;
    }
}
