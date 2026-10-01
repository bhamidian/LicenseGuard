using FluentValidation;
using LicenseGuard.Application.Features.Auth.Records;

namespace LicenseGuard.Application.Features.Auth.Commands.RequestLoginCode;

public sealed class RequestLoginCodeCommandValidator : AbstractValidator<RequestLoginCodeCommand>
{
    public RequestLoginCodeCommandValidator()
    {
        RuleFor(command => command.Channel).IsInEnum();
        RuleFor(command => command.Contact).NotEmpty().MaximumLength(256);
        When(command => command.Channel == LoginCodeChannel.Email, () =>
            RuleFor(command => command.Contact).EmailAddress());
        When(command => command.Channel == LoginCodeChannel.Sms, () =>
            RuleFor(command => command.Contact).Matches("^\\+[1-9][0-9]{7,14}$")
                .WithMessage("Phone number must use E.164 format, for example +14155552671."));
    }
}
