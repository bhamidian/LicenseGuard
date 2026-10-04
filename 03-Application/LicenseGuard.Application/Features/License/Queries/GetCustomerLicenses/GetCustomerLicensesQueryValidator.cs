using FluentValidation;

namespace LicenseGuard.Application.Features.License.Queries.GetCustomerLicenses;

public sealed class GetCustomerLicensesQueryValidator : AbstractValidator<GetCustomerLicensesQuery>
{
    public GetCustomerLicensesQueryValidator()
    {
        RuleFor(query => query.AppUserId).NotEmpty();
        RuleFor(query => query.Page).InclusiveBetween(1, 1_000_000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
