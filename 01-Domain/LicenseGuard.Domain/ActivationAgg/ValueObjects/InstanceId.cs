using LicenseGuard.Domain.Common.Entities;

namespace LicenseGuard.Domain.ActivationAgg.ValueObjects
{
    public sealed class InstanceId : ValueObject
    {
        protected override IEnumerable<object> GetEqualityComponents()
        {
            throw new NotImplementedException();
        }
    
        
    }
}