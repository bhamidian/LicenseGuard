namespace LicenseGuard.Domain.Entities
{
    public abstract class User : BaseEntity
    {
        public Guid AppUserId { get; private set; }
    }
}