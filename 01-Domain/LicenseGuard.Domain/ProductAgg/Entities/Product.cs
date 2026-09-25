using LicenseGuard.Domain.Common.Entities;

namespace LicenseGuard.Domain.ProductAgg.Entities
{
    public class Product : BaseEntity
    {
        public string Name {get; private set;} = null!;
    }
}