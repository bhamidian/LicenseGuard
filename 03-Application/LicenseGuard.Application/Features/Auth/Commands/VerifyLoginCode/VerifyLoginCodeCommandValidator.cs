using FluentValidation;
using LicenseGuard.Application.Features.Auth.Records;

namespace LicenseGuard.Application.Features.Auth.Commands.VerifyLoginCode;

public sealed class VerifyLoginCodeCommandValidator : AbstractValidator<VerifyLoginCodeCommand>
{
    public VerifyLoginCodeCommandValidator()
    {
        RuleFor(command => command.Channel).IsInEnum();
        RuleFor(command => command.Contact).NotEmpty().MaximumLength(256);
        RuleFor(command => command.Code).Matches("^[0-9]{6}$");
        When(command => command.Channel == LoginCodeChannel.Email, () =>
            RuleFor(command => command.Contact).EmailAddress());
        When(command => command.Channel == LoginCodeChannel.Sms, () =>
            RuleFor(command => command.Contact).Matches("^\\+[1-9][0-9]{7,14}$"));
    }
}
