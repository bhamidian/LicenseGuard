using FluentValidation;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.Subscription.Queries.GetSubscriptionDetails;

public sealed class GetSubscriptionDetailsQueryHandler(
    IValidator<GetSubscriptionDetailsQuery> validator,
    ISubscriptionRepository subscriptions,
    ICustomerRepository customers)
    : IRequestHandler<GetSubscriptionDetailsQuery, ResultDto<SubscriptionDetailsResponse>>
{
    public async Task<ResultDto<SubscriptionDetailsResponse>> Handle(
        GetSubscriptionDetailsQuery query, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<SubscriptionDetailsResponse>.Fail("Subscription lookup request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        Guid? customerId = null;
        if (query.CustomerAppUserId is { } appUserId)
        {
            var customer = await customers.GetByAppUserIdAsync(appUserId, cancellationToken);
            if (customer is null)
                return ResultDto<SubscriptionDetailsResponse>.Fail("Subscription was not found.",
                    failureKind: ResultFailureKind.NotFound);
            customerId = customer.Id;
        }

        var subscription = await subscriptions.GetDetailsAsync(query.SubscriptionId,
            customerId, cancellationToken);
        if (subscription is null)
            return ResultDto<SubscriptionDetailsResponse>.Fail("Subscription was not found.",
                failureKind: ResultFailureKind.NotFound);

        var now = DateTime.UtcNow;
        var effectiveStatus = subscription.Status is SubscriptionStatusEnum.Canceled or SubscriptionStatusEnum.Expired
            ? subscription.Status
            : subscription.EndDate <= now ? SubscriptionStatusEnum.Expired : subscription.Status;
        var remainingDays = effectiveStatus is SubscriptionStatusEnum.Canceled or SubscriptionStatusEnum.Expired
            ? 0
            : Math.Max(0, (int)Math.Ceiling((subscription.EndDate - now).TotalDays));

        var history = subscription.Renewals
            .OrderByDescending(renewal => renewal.RenewedAt)
            .ThenByDescending(renewal => renewal.Id)
            .Select(renewal => new SubscriptionRenewalItemResponse(renewal.Id, renewal.RenewedAt,
                renewal.Amount, renewal.PreviousExpirationDate, renewal.NewExpirationDate))
            .ToArray();

        var response = new SubscriptionDetailsResponse(subscription.Id, subscription.CustomerId,
            subscription.ProductId, subscription.Product.Name, subscription.PlanId, subscription.Plan.Name,
            effectiveStatus, subscription.StartDate, subscription.EndDate, remainingDays,
            subscription.CancelledAt, subscription.CurrentLicenseId, history);
        return ResultDto<SubscriptionDetailsResponse>.Success("Subscription retrieved.", response);
    }
}
