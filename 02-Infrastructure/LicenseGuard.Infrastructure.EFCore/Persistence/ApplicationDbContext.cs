
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Infrastructure.EFCore.Persistence
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
    {
        
    }
}