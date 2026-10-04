using FluentValidation;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductPlans;

public sealed class GetProductPlansQueryHandler(
    IValidator<GetProductPlansQuery> validator,
    IProductCatalogRepository catalog)
    : IRequestHandler<GetProductPlansQuery, ResultDto<ProductPlansResponse>>
{
    public async Task<ResultDto<ProductPlansResponse>> Handle(
        GetProductPlansQuery query, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<ProductPlansResponse>.Fail("Product plan request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        var product = await catalog.GetPlansByProductIdAsync(query.ProductId, cancellationToken);
        if (product is null)
            return ResultDto<ProductPlansResponse>.Fail("Product was not found.",
                failureKind: ResultFailureKind.NotFound);

        var plans = product.Plans.Select(plan => new PlanCatalogItemResponse(
            plan.PlanId, plan.Name, plan.Description, plan.Price,
            plan.Features.Select(feature => new PlanFeatureCatalogItemResponse(
                feature.FeatureId, feature.Code, feature.Name, feature.Description)).ToArray())).ToArray();

        return ResultDto<ProductPlansResponse>.Success("Product plans retrieved.",
            new ProductPlansResponse(product.ProductId, plans));
    }
}
