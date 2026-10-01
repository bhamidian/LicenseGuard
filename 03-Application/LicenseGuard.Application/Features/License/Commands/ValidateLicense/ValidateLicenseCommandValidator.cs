using FluentValidation;

namespace LicenseGuard.Application.Features.License.Commands.ValidateLicense;

public sealed class ValidateLicenseCommandValidator : AbstractValidator<ValidateLicenseCommand>
{
    public ValidateLicenseCommandValidator()
    {
        RuleFor(command => command.LicenseKey)
            .NotEmpty().Length(64)
            .Matches("^[A-Fa-f0-9]{64}$");
        RuleFor(command => command.MachineId)
            .NotEmpty().MaximumLength(256)
            .Must(value => !value.Any(char.IsControl));
        RuleFor(command => command.InstanceId)
            .NotEmpty().MaximumLength(256)
            .Must(value => !value.Any(char.IsControl));
    }
}
