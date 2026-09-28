namespace LicenseGuard.Domain.Entities
{
    public class SubscriptionRenewal : BaseEntity
    {
        private SubscriptionRenewal() { }

        public SubscriptionRenewal(Guid subscriptionId, decimal amount, DateTime previousExpirationDate, DateTime newExpirationDate, DateTime? renewedAt = null)
        {
            if (subscriptionId == Guid.Empty)
                throw new Exceptions.DomainValidationException("Subscription ID cannot be empty.");
            if (amount < 0)
                throw new Exceptions.DomainValidationException("Renewal amount cannot be negative.");
            previousExpirationDate = EnsureUtc(previousExpirationDate);
            newExpirationDate = EnsureUtc(newExpirationDate);
            if (newExpirationDate <= previousExpirationDate)
                throw new Exceptions.DomainValidationException("New expiration must be later than previous expiration.");
            SubscriptionId = subscriptionId;
            Amount = amount;
            PreviousExpirationDate = previousExpirationDate;
            NewExpirationDate = newExpirationDate;
            RenewedAt = EnsureUtc(renewedAt ?? DateTime.UtcNow);
        }

        public decimal Amount { get; private set; }
        public DateTime RenewedAt { get; private set; }
        public Guid SubscriptionId { get; private set; }
        public DateTime NewExpirationDate { get; private set; }
        public DateTime PreviousExpirationDate { get; private set; }
    }
}
