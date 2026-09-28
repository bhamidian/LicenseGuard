using LicenseGuard.Domain.Exceptions;

namespace LicenseGuard.Domain.Entities
{
    public class LicenseFeature : BaseEntity
    {
        private LicenseFeature() { }

        public LicenseFeature(Guid licenseId, Feature feature, bool isEnabled = true)
        {
            if (licenseId == Guid.Empty)
                throw new DomainValidationException("License ID cannot be empty.");
            ArgumentNullException.ThrowIfNull(feature);
            LicenseId = licenseId;
            FeatureId = feature.Id;
            Feature = feature;
            IsEnabled = isEnabled;
        }

        public Guid LicenseId { get; private set; }
        public License License { get; private set; } = null!;
        public Guid FeatureId { get; private set; }
        public Feature Feature { get; private set; } = null!;
        public bool IsEnabled { get; private set; } = true;

        public void Enable() => IsEnabled = true;
        public void Disable() => IsEnabled = false;
        public void SetEnabled(bool enabled) => IsEnabled = enabled;

        internal void AttachToLicense(License license)
        {
            ArgumentNullException.ThrowIfNull(license);
            if (license.Id != LicenseId)
                throw new Exceptions.DomainRuleViolationException("Feature grant belongs to another license.");
            License = license;
        }
    }
}
