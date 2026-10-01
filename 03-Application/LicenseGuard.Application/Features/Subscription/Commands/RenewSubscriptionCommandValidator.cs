using FluentValidation;

namespace LicenseGuard.Application.Features.Subscription.Commands;

public sealed class RenewSubscriptionCommandValidator : AbstractValidator<RenewSubscriptionCommand>
{
    public RenewSubscriptionCommandValidator()
    {
        RuleFor(command => command.SubscriptionId).NotEmpty();
        RuleFor(command => command.RenewedByUserId).NotEmpty();
        RuleFor(command => command.Amount).GreaterThanOrEqualTo(0);
        RuleFor(command => command.NewExpirationDate).NotEmpty();
    }
}
