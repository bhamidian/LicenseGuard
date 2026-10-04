using FluentValidation;

namespace LicenseGuard.Application.Features.License.Commands.DeactivateLicenseActivation;

public sealed class DeactivateLicenseActivationCommandValidator
    : AbstractValidator<DeactivateLicenseActivationCommand>
{
    public DeactivateLicenseActivationCommandValidator()
    {
        RuleFor(command => command.LicenseId).NotEmpty();
        RuleFor(command => command.ActivationId).NotEmpty();
        RuleFor(command => command.AdminId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Reason).MaximumLength(500);
    }
}
