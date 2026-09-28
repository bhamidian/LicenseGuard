namespace LicenseGuard.Domain.Entities
{
    public class LicenseLimit : BaseEntity
    {
        private LicenseLimit() { }

        public LicenseLimit(Guid licenseId, string code, decimal value, string? unit = null)
        {
            if (licenseId == Guid.Empty)
                throw new Exceptions.DomainValidationException("License ID cannot be empty.");
            if (value < 0)
                throw new Exceptions.DomainValidationException("Limit value cannot be negative.");
            LicenseId = licenseId;
            Code = DomainText.Required(code, 100, nameof(code)).ToLowerInvariant();
            Unit = DomainText.Optional(unit, 32, nameof(unit));
            Value = value;
        }

        public Guid LicenseId { get; private set; }
        public License License { get; private set; } = null!;
        public string Code { get; private set; } = null!;
        public decimal Value { get; private set; }
        public string? Unit { get; private set; }

        public void SetValue(decimal value, string? unit = null)
        {
            if (value < 0)
                throw new Exceptions.DomainValidationException("Limit value cannot be negative.");
            Value = value;
            Unit = DomainText.Optional(unit, 32, nameof(unit));
            Touch();
        }

        internal void AttachToLicense(License license)
        {
            ArgumentNullException.ThrowIfNull(license);
            if (license.Id != LicenseId)
                throw new Exceptions.DomainRuleViolationException("Limit belongs to another license.");
            License = license;
        }
    }
}
