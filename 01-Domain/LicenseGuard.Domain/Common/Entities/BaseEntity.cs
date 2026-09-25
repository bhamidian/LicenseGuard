namespace LicenseGuard.Domain.Common.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } 
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public Guid CreatedBy { get; set; }
    }
}