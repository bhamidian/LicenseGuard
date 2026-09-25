using Microsoft.AspNetCore.Identity;

namespace LicenseGuard.Domain.Entities
{
    public class AppUser : IdentityRole<Guid>
    {
        public Guid UserId { get; private set; }
    }
}