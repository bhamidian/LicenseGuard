using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfCustomerRepository(ApplicationDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default) =>
        dbContext.Customers.SingleOrDefaultAsync(customer => customer.AppUserId == appUserId, cancellationToken);

    public Customer Create(CreateCustomerProfileRecord record)
    {
        var customer = new Customer(record.AppUserId);
        dbContext.Customers.Add(customer);
        return customer;
    }
}
