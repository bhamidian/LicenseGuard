namespace LicenseGuard.Domain.Entities
{
    public class ProductFeature : BaseEntity
    {
        private ProductFeature() { }

        public ProductFeature(Product product, Feature feature)
        {
            ArgumentNullException.ThrowIfNull(product);
            ArgumentNullException.ThrowIfNull(feature);
            ProductId = product.Id;
            FeatureId = feature.Id;
            Product = product;
            Feature = feature;
        }

        public Guid FeatureId { get; private set; }
        public Guid ProductId { get; private set; }
        public Feature Feature { get; private set; } = null!;
        public Product Product { get; private set; } = null!;
    }
}
