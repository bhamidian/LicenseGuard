using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using LicenseGuard.Application.Features.Subscription.Commands;
using LicenseGuard.Application.Features.Subscription.Queries.GetSubscriptionDetails;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Endpoint.WebApi.Records;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LicenseGuard.Endpoint.WebApi.Controllers;

[ApiController]
[Route("api/subscriptions")]
public sealed class SubscriptionController(ISender sender) : ControllerBase
{
    [HttpGet("{subscriptionId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDetails(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubscriptionDetailsQuery(subscriptionId), cancellationToken);
        return result.IsSuccess ? Ok(result) : LookupFailure(result);
    }

    [HttpGet("mine/{subscriptionId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine(Guid subscriptionId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var appUserId))
            return Forbid();

        var result = await sender.Send(new GetSubscriptionDetailsQuery(subscriptionId, appUserId),
            cancellationToken);
        return result.IsSuccess ? Ok(result) : LookupFailure(result);
    }

    [HttpPost("{subscriptionId:guid}/renew")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Renew(
        Guid subscriptionId,
        RenewSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
            return Forbid();

        var result = await sender.Send(new RenewSubscriptionCommand(
            subscriptionId, adminId, request.Amount, request.NewExpirationDate), cancellationToken);

        if (result.IsSuccess) return Ok(result);
        return result.FailureKind switch
        {
            ResultFailureKind.NotFound => NotFound(result),
            ResultFailureKind.Conflict => Conflict(result),
            _ => BadRequest(result)
        };
    }

    private IActionResult LookupFailure<T>(ResultDto<T> result) =>
        result.FailureKind == ResultFailureKind.NotFound ? NotFound(result) : BadRequest(result);
}
