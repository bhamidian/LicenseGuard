using FluentValidation;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductFeatures;

public sealed class GetProductFeaturesQueryHandler(
    IValidator<GetProductFeaturesQuery> validator,
    IProductCatalogRepository catalog)
    : IRequestHandler<GetProductFeaturesQuery, ResultDto<ProductFeaturesResponse>>
{
    public async Task<ResultDto<ProductFeaturesResponse>> Handle(
        GetProductFeaturesQuery query, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<ProductFeaturesResponse>.Fail("Product feature request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        var product = await catalog.GetFeaturesByProductIdAsync(query.ProductId, cancellationToken);
        if (product is null)
            return ResultDto<ProductFeaturesResponse>.Fail("Product was not found.",
                failureKind: ResultFailureKind.NotFound);

        var features = product.Features.Select(feature => new ProductFeatureItemResponse(
            feature.FeatureId, feature.Code, feature.Name, feature.Description)).ToArray();
        return ResultDto<ProductFeaturesResponse>.Success("Product features retrieved.",
            new ProductFeaturesResponse(product.ProductId, features));
    }
}
