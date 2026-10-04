namespace LicenseGuard.Domain.Records;

public sealed record ProductCatalogItemRecord(Guid ProductId, string Code, string Name, string Description);

public sealed record PlanCatalogItemRecord(Guid PlanId, Guid ProductId, string Name, string Description,
    decimal Price, IReadOnlyCollection<ProductFeatureCatalogItemRecord> Features);

public sealed record ProductFeatureCatalogItemRecord(Guid FeatureId, string Code, string Name, string Description);

public sealed record ProductPlansCatalogRecord(Guid ProductId, IReadOnlyCollection<PlanCatalogItemRecord> Plans);

public sealed record ProductFeaturesCatalogRecord(Guid ProductId,
    IReadOnlyCollection<ProductFeatureCatalogItemRecord> Features);
