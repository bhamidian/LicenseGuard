namespace LicenseGuard.Domain.Entities
{
    public abstract class BaseEntity
    {
        protected BaseEntity() : this(Guid.Empty) { }

        protected BaseEntity(Guid createdBy)
        {
            Id = Guid.NewGuid();
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public Guid Id { get; private set; }
        public Guid CreatedBy { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public void SetCreatedBy(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new Exceptions.DomainValidationException("Creator ID cannot be empty.");

            CreatedBy = userId;
            Touch();
        }

        public void Deactivate()
        {
            if (IsDeleted)
                throw new Exceptions.DomainRuleViolationException("A deleted entity cannot be deactivated.");

            IsActive = false;
            Touch();
        }

        public void Reactivate()
        {
            if (IsDeleted)
                throw new Exceptions.DomainRuleViolationException("A deleted entity cannot be reactivated.");

            IsActive = true;
            Touch();
        }

        public void SoftDelete(DateTime? at = null)
        {
            if (IsDeleted)
                return;

            DeletedAt = EnsureUtc(at ?? DateTime.UtcNow);
            IsDeleted = true;
            IsActive = false;
            Touch(DeletedAt.Value);
        }

        public void Restore()
        {
            if (!IsDeleted)
                throw new Exceptions.DomainRuleViolationException("Only a deleted entity can be restored.");

            IsDeleted = false;
            IsActive = true;
            DeletedAt = null;
            Touch();
        }

        protected void Touch(DateTime? at = null) => UpdatedAt = EnsureUtc(at ?? DateTime.UtcNow);

        protected static DateTime EnsureUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
