using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using MediatR;

namespace LicenseGuard.Application.Features.Subscription.Queries.GetSubscriptionDetails;

public sealed record GetSubscriptionDetailsQuery(Guid SubscriptionId, Guid? CustomerAppUserId = null)
    : IRequest<ResultDto<SubscriptionDetailsResponse>>;

public sealed record SubscriptionDetailsResponse(
    Guid SubscriptionId,
    Guid CustomerId,
    Guid ProductId,
    string ProductName,
    Guid PlanId,
    string PlanName,
    SubscriptionStatusEnum Status,
    DateTime StartDate,
    DateTime EndDate,
    int RemainingDays,
    DateTime? CancelledAt,
    Guid? CurrentLicenseId,
    IReadOnlyCollection<SubscriptionRenewalItemResponse> RenewalHistory);

public sealed record SubscriptionRenewalItemResponse(
    Guid RenewalId,
    DateTime RenewedAt,
    decimal Amount,
    DateTime PreviousExpirationDate,
    DateTime NewExpirationDate);
