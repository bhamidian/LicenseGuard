namespace LicenseGuard.Domain.Entities
{
    public class LicenseLimit : BaseEntity
    {
        public Guid LicenseId { get; private set; }
        public License License { get; private set; } = null!;
        public string Code { get; private set; } = null!;
        public decimal Value { get; private set; }
        public string? Unit { get; private set; }
    }
}
