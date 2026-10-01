using FluentValidation;

namespace LicenseGuard.Application.Features.License.Commands.ActivateLicense;

public sealed class ActivateLicenseCommandValidator : AbstractValidator<ActivateLicenseCommand>
{
    public ActivateLicenseCommandValidator()
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
        RuleFor(command => command.IpAddress).NotEmpty().MaximumLength(64);
    }
}
