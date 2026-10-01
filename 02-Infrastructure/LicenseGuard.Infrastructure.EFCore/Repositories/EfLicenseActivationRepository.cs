using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.EFCore.Persistence;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfLicenseActivationRepository(ApplicationDbContext dbContext) : ILicenseActivationRepository
{
    public LicenseActivation Create(CreateLicenseActivationRecord record)
    {
        var activation = new LicenseActivation(record.LicenseId, record.MachineId,
            record.InstanceId, record.IpAddress, record.ActivatedAt);
        dbContext.LicenseActivations.Add(activation);
        return activation;
    }
}
