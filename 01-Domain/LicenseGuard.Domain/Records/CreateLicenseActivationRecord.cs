using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Domain.Records;

public sealed record CreateLicenseActivationRecord(
    Guid LicenseId,
    MachineId MachineId,
    InstanceId InstanceId,
    string IpAddress,
    DateTime ActivatedAt);
