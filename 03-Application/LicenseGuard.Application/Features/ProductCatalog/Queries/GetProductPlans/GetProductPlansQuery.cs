using LicenseGuard.Domain.Dtos;
using MediatR;

namespace LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductPlans;

public sealed record GetProductPlansQuery(Guid ProductId) : IRequest<ResultDto<ProductPlansResponse>>;

public sealed record ProductPlansResponse(Guid ProductId, IReadOnlyCollection<PlanCatalogItemResponse> Plans);

public sealed record PlanCatalogItemResponse(Guid PlanId, string Name, string Description, decimal Price,
    IReadOnlyCollection<PlanFeatureCatalogItemResponse> Features);

public sealed record PlanFeatureCatalogItemResponse(Guid FeatureId, string Code, string Name, string Description);
