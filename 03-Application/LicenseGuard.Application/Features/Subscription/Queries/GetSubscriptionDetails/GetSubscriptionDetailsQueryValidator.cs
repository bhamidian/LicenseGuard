using FluentValidation;

namespace LicenseGuard.Application.Features.Subscription.Queries.GetSubscriptionDetails;

public sealed class GetSubscriptionDetailsQueryValidator : AbstractValidator<GetSubscriptionDetailsQuery>
{
    public GetSubscriptionDetailsQueryValidator()
    {
        RuleFor(query => query.SubscriptionId).NotEmpty();
        RuleFor(query => query.CustomerAppUserId).Must(id => id is null || id != Guid.Empty);
    }
}
