using LicenseGuard.Domain.Common.Entities;

namespace LicenseGuard.Domain.ActivationAgg.ValueObjects
{
    public sealed class MachineId : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            throw new NotImplementedException();
        }
    }
}