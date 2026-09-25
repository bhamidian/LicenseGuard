namespace LicenseGuard.Domain.Entities
{
    public class LicenseFeature : BaseEntity
    {
        public Guid LicenseId { get; private set; }
        public License License { get; private set; } = null!;
        public Guid FeatureId { get; private set; }
        public Feature Feature { get; private set; } = null!;
        public bool IsEnabled { get; private set; } = true;
    }
}
