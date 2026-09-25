namespace LicenseGuard.Domain.Entities
{
    public class Feature : BaseEntity
    {
        public ICollection<ProductFeature> ProductFeatures { get; private set; } = new List<ProductFeature>();
        public ICollection<PlanFeature> PlanFeatures { get; private set; } = new List<PlanFeature>();
        public ICollection<LicenseFeature> LicenseFeatures { get; private set; } = new List<LicenseFeature>();
        public string Name { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string Code { get; private set; } = null!;
    }
}
