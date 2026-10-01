using FluentValidation;

namespace LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand;

public sealed class CreateLicenseCommandValidator : AbstractValidator<CreateLicenseCommand>
{
    public CreateLicenseCommandValidator()
    {
        RuleFor(command => command.SubscriptionId).NotEmpty();
        RuleFor(command => command.IssuedByAdminId).NotEmpty();
        RuleFor(command => command.MaxActivations).GreaterThan(0);
    }
}
