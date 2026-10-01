using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default);
    Customer Create(CreateCustomerProfileRecord record);
}
