using FluentValidation;

namespace LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductFeatures;

public sealed class GetProductFeaturesQueryValidator : AbstractValidator<GetProductFeaturesQuery>
{
    public GetProductFeaturesQueryValidator() => RuleFor(query => query.ProductId).NotEmpty();
}
