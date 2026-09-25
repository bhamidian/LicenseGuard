namespace LicenseGuard.Domain.Entities
{
    public class Customer : User
    {
        public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
    }
}
