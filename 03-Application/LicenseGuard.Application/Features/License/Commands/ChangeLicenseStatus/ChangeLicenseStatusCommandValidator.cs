using FluentValidation;

namespace LicenseGuard.Application.Features.License.Commands.ChangeLicenseStatus;

public sealed class ChangeLicenseStatusCommandValidator : AbstractValidator<ChangeLicenseStatusCommand>
{
    public ChangeLicenseStatusCommandValidator()
    {
        RuleFor(command => command.LicenseId).NotEmpty();
        RuleFor(command => command.AdminId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Action).IsInEnum();
        RuleFor(command => command.Reason).MaximumLength(1000);
    }
}
