using Microsoft.AspNetCore.Identity;

namespace LicenseGuard.Domain.Entities;

/// <summary>
/// Authentication account shared by domain profiles such as Customer and Admin.
/// Identity roles are represented by IdentityRole, not by this account entity.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
}
