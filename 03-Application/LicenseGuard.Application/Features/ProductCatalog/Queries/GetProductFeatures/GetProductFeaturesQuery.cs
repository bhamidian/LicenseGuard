using LicenseGuard.Domain.Dtos;
using MediatR;

namespace LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductFeatures;

public sealed record GetProductFeaturesQuery(Guid ProductId)
    : IRequest<ResultDto<ProductFeaturesResponse>>;

public sealed record ProductFeaturesResponse(Guid ProductId,
    IReadOnlyCollection<ProductFeatureItemResponse> Features);

public sealed record ProductFeatureItemResponse(Guid FeatureId, string Code, string Name, string Description);
