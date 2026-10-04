using FluentValidation;

namespace LicenseGuard.Application.Features.License.Commands.ExpireLicenses;

public sealed class ExpireLicensesCommandValidator : AbstractValidator<ExpireLicensesCommand>
{
    public ExpireLicensesCommandValidator() =>
        RuleFor(command => command.BatchSize).InclusiveBetween(1, 1000);
}
