using LicenseGuard.Domain.Dtos;
using MediatR;

namespace LicenseGuard.Application.Features.Subscription.Commands;

public sealed record RenewSubscriptionCommand(
    Guid SubscriptionId,
    Guid RenewedByUserId,
    decimal Amount,
    DateTime NewExpirationDate) : IRequest<ResultDto<RenewSubscriptionResponse>>;

public sealed record RenewSubscriptionResponse(
    Guid SubscriptionId,
    Guid? LicenseId,
    DateTime SubscriptionExpirationDate,
    DateTime? LicenseExpirationDate,
    string? LicenseSignature);
