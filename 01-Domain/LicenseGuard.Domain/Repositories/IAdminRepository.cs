namespace LicenseGuard.Domain.Repositories;

public interface IAdminRepository
{
    Task<Guid?> GetProfileIdByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default);
}
