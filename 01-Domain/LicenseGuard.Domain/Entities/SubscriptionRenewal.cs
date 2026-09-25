namespace LicenseGuard.Domain.Entities
{
    public class SubscriptionRenewal : BaseEntity
    {
        public Guid SubscriptionId { get; private set; }
        public DateTime RenewedAt { get; private set; }
        public DateTime PreviousExpirationDate { get; private set; }
        public DateTime NewExpirationDate { get; private set; }
        public decimal Amount { get; private set; }
    }
}