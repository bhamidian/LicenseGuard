using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Domain.Repositories;

public interface IAppUserRepository
{
    Task<AppUser?> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<AppUser?> CreatePendingAsync(CreatePendingAppUserRecord record, CancellationToken cancellationToken = default);
}
