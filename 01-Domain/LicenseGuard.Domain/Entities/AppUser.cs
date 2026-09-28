using Microsoft.AspNetCore.Identity;

namespace LicenseGuard.Domain.Entities
{
    public class AppUser : IdentityRole<Guid>
    {
        private AppUser() { }

        public AppUser(string roleName, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new Exceptions.DomainValidationException("Role name is required.");
            SetUserId(userId);
            Id = Guid.NewGuid();
            Name = roleName.Trim();
            NormalizedName = Name.ToUpperInvariant();
        }

        public Guid UserId { get; private set; }

        public void SetUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new Exceptions.DomainValidationException("User ID cannot be empty.");
            UserId = userId;
        }

        public void RenameRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new Exceptions.DomainValidationException("Role name is required.");
            Name = roleName.Trim();
            NormalizedName = Name.ToUpperInvariant();
        }
    }
}
