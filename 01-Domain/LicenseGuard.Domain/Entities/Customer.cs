namespace LicenseGuard.Domain.Entities
{
    public class Customer : User
    {
        private Customer() { }

        public Customer(Guid appUserId) : base(appUserId) { }

        public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();

        public void AddSubscription(Subscription subscription)
        {
            ArgumentNullException.ThrowIfNull(subscription);
            if (subscription.CustomerId != Id)
                throw new Exceptions.DomainRuleViolationException("Subscription belongs to another customer.");
            if (Subscriptions.All(item => item.Id != subscription.Id))
                Subscriptions.Add(subscription);
        }
    }
}
