using FluentValidation;

namespace LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductPlans;

public sealed class GetProductPlansQueryValidator : AbstractValidator<GetProductPlansQuery>
{
    public GetProductPlansQueryValidator() => RuleFor(query => query.ProductId).NotEmpty();
}
