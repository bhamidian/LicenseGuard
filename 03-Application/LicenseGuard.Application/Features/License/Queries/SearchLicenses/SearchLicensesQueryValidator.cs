using FluentValidation;

namespace LicenseGuard.Application.Features.License.Queries.SearchLicenses;

public sealed class SearchLicensesQueryValidator : AbstractValidator<SearchLicensesQuery>
{
    public SearchLicensesQueryValidator()
    {
        RuleFor(query => query.LicenseKey)
            .Must(key => string.IsNullOrWhiteSpace(key) ||
                         (key.Length == 64 && key.All(Uri.IsHexDigit)))
            .WithMessage("License key search must be exactly 64 hexadecimal characters.");
        RuleFor(query => query.ExpiresWithinDays).InclusiveBetween(1, 365).When(query => query.ExpiresWithinDays.HasValue);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
