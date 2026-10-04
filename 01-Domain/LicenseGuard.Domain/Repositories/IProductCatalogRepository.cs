using LicenseGuard.Domain.Records;

namespace LicenseGuard.Domain.Repositories;

public interface IProductCatalogRepository
{
    Task<IReadOnlyCollection<ProductCatalogItemRecord>> GetProductsAsync(
        CancellationToken cancellationToken = default);
    Task<ProductPlansCatalogRecord?> GetPlansByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default);
    Task<ProductFeaturesCatalogRecord?> GetFeaturesByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default);
}
