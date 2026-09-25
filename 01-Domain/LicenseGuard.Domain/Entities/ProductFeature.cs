namespace LicenseGuard.Domain.Entities
{
    public class ProductFeature : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Product Product { get; private set; } = null!;
        public Guid FeatureId { get; private set; }
        public Feature Feature { get; private set; } = null!;
    }
}
