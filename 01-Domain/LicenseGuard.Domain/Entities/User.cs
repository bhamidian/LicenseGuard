namespace LicenseGuard.Domain.Entities
{
    public abstract class User : BaseEntity
    {
        protected User() { }

        protected User(Guid appUserId)
        {
            SetAppUserId(appUserId);
        }

        public Guid AppUserId { get; private set; }

        public void SetAppUserId(Guid appUserId)
        {
            if (appUserId == Guid.Empty)
                throw new Exceptions.DomainValidationException("Application user ID cannot be empty.");

            AppUserId = appUserId;
            Touch();
        }
    }
}
