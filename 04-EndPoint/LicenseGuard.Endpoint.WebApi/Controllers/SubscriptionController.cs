using LicenseGuard.Application.Features.Subscription.Commands;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Endpoint.WebApi.Records;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LicenseGuard.Endpoint.WebApi.Controllers;

[ApiController]
[Route("api/subscriptions")]
public sealed class SubscriptionController(IMediator mediator) : ControllerBase
{
    [HttpPost("{subscriptionId:guid}/renew")]
    public async Task<IActionResult> Renew(
        Guid subscriptionId,
        RenewSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RenewSubscriptionCommand(
            subscriptionId, request.RenewedByUserId, request.Amount, request.NewExpirationDate), cancellationToken);

        if (result.IsSuccess) return Ok(result);
        return result.FailureKind switch
        {
            ResultFailureKind.NotFound => NotFound(result),
            ResultFailureKind.Conflict => Conflict(result),
            _ => BadRequest(result)
        };
    }
}
