using FluentValidation;

namespace LicenseGuard.Application.Features.License.Queries.GetLicenseDetails;

public sealed class GetLicenseDetailsQueryValidator : AbstractValidator<GetLicenseDetailsQuery>
{
    public GetLicenseDetailsQueryValidator() => RuleFor(query => query.LicenseId).NotEmpty();
}
