using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LicenseGuard.Infrastructure.EFCore.Repositories;

public sealed class EfProductCatalogRepository(ApplicationDbContext dbContext) : IProductCatalogRepository
{
    public async Task<IReadOnlyCollection<ProductCatalogItemRecord>> GetProductsAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive && !product.IsDeleted)
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Select(product => new ProductCatalogItemRecord(
                product.Id, product.Code, product.Name, product.Description))
            .ToArrayAsync(cancellationToken);

    public async Task<ProductPlansCatalogRecord?> GetPlansByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        var productExists = await dbContext.Products.AsNoTracking()
            .AnyAsync(product => product.Id == productId && product.IsActive && !product.IsDeleted,
                cancellationToken);
        if (!productExists) return null;

        var plans = await dbContext.Plans
            .AsNoTracking()
            .Where(plan => plan.ProductId == productId && plan.IsActive && !plan.IsDeleted)
            .Include(plan => plan.PlanFeatures)
                .ThenInclude(link => link.Feature)
            .OrderBy(plan => plan.Name)
            .ThenBy(plan => plan.Id)
            .ToArrayAsync(cancellationToken);

        var items = plans.Select(plan => new PlanCatalogItemRecord(
            plan.Id, plan.ProductId, plan.Name, plan.Description, plan.Price,
            plan.PlanFeatures
                .Where(link => link.IsActive && !link.IsDeleted && link.Feature.IsActive && !link.Feature.IsDeleted)
                .OrderBy(link => link.Feature.Name)
                .Select(link => new ProductFeatureCatalogItemRecord(
                    link.FeatureId, link.Feature.Code, link.Feature.Name, link.Feature.Description))
                .ToArray())).ToArray();

        return new ProductPlansCatalogRecord(productId, items);
    }

    public async Task<ProductFeaturesCatalogRecord?> GetFeaturesByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        var productExists = await dbContext.Products.AsNoTracking()
            .AnyAsync(product => product.Id == productId && product.IsActive && !product.IsDeleted,
                cancellationToken);
        if (!productExists) return null;

        var features = await dbContext.ProductFeatures
            .AsNoTracking()
            .Where(link => link.ProductId == productId && link.IsActive && !link.IsDeleted &&
                link.Feature.IsActive && !link.Feature.IsDeleted)
            .OrderBy(link => link.Feature.Name)
            .ThenBy(link => link.FeatureId)
            .Select(link => new ProductFeatureCatalogItemRecord(
                link.FeatureId, link.Feature.Code, link.Feature.Name, link.Feature.Description))
            .ToArrayAsync(cancellationToken);

        return new ProductFeaturesCatalogRecord(productId, features);
    }
}
