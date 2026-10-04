using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfAdminRepository(ApplicationDbContext dbContext) : IAdminRepository
{
    public Task<Guid?> GetProfileIdByAppUserIdAsync(Guid appUserId,
        CancellationToken cancellationToken = default) =>
        dbContext.Admins
            .Where(admin => admin.AppUserId == appUserId)
            .Select(admin => (Guid?)admin.Id)
            .SingleOrDefaultAsync(cancellationToken);
}
