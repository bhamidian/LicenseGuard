using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using LicenseGuard.Application.Features.License.Commands.ActivateLicense;
using LicenseGuard.Application.Features.License.Commands.ChangeLicenseStatus;
using LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand;
using LicenseGuard.Application.Features.License.Commands.ValidateLicense;
using LicenseGuard.Application.Features.License.Queries.SearchLicenses;
using LicenseGuard.Application.Features.License.Queries.GetLicenseDetails;
using LicenseGuard.Application.Features.License.Commands.DeactivateLicenseActivation;
using LicenseGuard.Application.Features.License.Queries.GetCustomerLicenses;
using LicenseGuard.Application.Features.License.Commands.UpdateLicense;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Endpoint.WebApi.Records;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LicenseGuard.Endpoint.WebApi.Controllers;

[ApiController]
[Route("api/licenses")]
public sealed class LicenseController(ISender sender, IAdminRepository admins) : ControllerBase
{
    [HttpGet("mine")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var appUserId))
            return Forbid();

        var result = await sender.Send(new GetCustomerLicensesQuery(appUserId, page, pageSize), cancellationToken);
        if (result.IsSuccess) return Ok(result);
        return result.FailureKind == ResultFailureKind.NotFound ? NotFound(result) : BadRequest(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Search([FromQuery] SearchLicensesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchLicensesQuery(request.LicenseKey, request.CustomerId,
            request.ProductId, request.PlanId, request.Status, request.ExpiresWithinDays,
            request.Page, request.PageSize), cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{licenseId:guid}")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> GetDetails(Guid licenseId, CancellationToken cancellationToken) =>
        GetDetailsResult(licenseId, cancellationToken);

    [HttpGet("{licenseId:guid}/activations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetActivations(Guid licenseId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLicenseDetailsQuery(licenseId), cancellationToken);
        return result.IsSuccess ? Ok(result.Data!.Activations) : DetailFailure(result);
    }

    [HttpPost("{licenseId:guid}/activations/{activationId:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateActivation(Guid licenseId, Guid activationId,
        DeactivateLicenseActivationRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var appUserId))
            return Forbid();

        var adminProfileId = await admins.GetProfileIdByAppUserIdAsync(appUserId, cancellationToken);
        if (adminProfileId is null)
            return Forbid();

        var result = await sender.Send(new DeactivateLicenseActivationCommand(licenseId, activationId,
            adminProfileId.Value, appUserId, request.Reason), cancellationToken);
        if (result.IsSuccess) return Ok(result);

        return result.FailureKind switch
        {
            ResultFailureKind.NotFound => NotFound(result),
            ResultFailureKind.Conflict => Conflict(result),
            _ => BadRequest(result)
        };
    }

    [HttpGet("{licenseId:guid}/history")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetHistory(Guid licenseId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLicenseDetailsQuery(licenseId), cancellationToken);
        return result.IsSuccess ? Ok(result.Data!.History) : DetailFailure(result);
    }

    [HttpGet("{licenseId:guid}/audit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAudit(Guid licenseId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLicenseDetailsQuery(licenseId), cancellationToken);
        return result.IsSuccess ? Ok(result.Data!.AuditLog) : DetailFailure(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CreateLicenseRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var appUserId))
            return Forbid();

        var adminProfileId = await admins.GetProfileIdByAppUserIdAsync(appUserId, cancellationToken);
        if (adminProfileId is null)
            return Forbid();

        var command = new CreateLicenseCommand(
            request.SubscriptionId, adminProfileId.Value, appUserId, request.MaxActivations, request.FeatureIds);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsSuccess) return Ok(result);

        return result.FailureKind switch
        {
            ResultFailureKind.NotFound => NotFound(result),
            ResultFailureKind.Conflict => Conflict(result),
            _ => BadRequest(result)
        };
    }

    [HttpPut("{licenseId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid licenseId, UpdateLicenseRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var appUserId))
            return Forbid();

        var adminProfileId = await admins.GetProfileIdByAppUserIdAsync(appUserId, cancellationToken);
        if (adminProfileId is null)
            return Forbid();

        var limits = request.Limits?.Select(limit =>
            new LicenseGuard.Domain.Records.LicenseLimitUpdateRecord(limit.Code, limit.Value, limit.Unit)).ToArray();
        var command = new UpdateLicenseCommand(licenseId, adminProfileId.Value, appUserId,
            request.Description, request.AutoRenewalEnabled, request.AutoRenewalPlan,
            request.MaxActivations, request.FeatureIds, limits, request.Reason);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsSuccess) return Ok(result);

        return result.FailureKind switch
        {
            ResultFailureKind.NotFound => NotFound(result),
            ResultFailureKind.Conflict => Conflict(result),
            _ => BadRequest(result)
        };
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate(
        ActivateLicenseRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new ActivateLicenseCommand(
            request.LicenseKey, request.MachineId, request.InstanceId, ipAddress);
        var result = await sender.Send(command, cancellationToken);
        if (result.IsSuccess) return Ok(result);

        return result.FailureKind switch
        {
            ResultFailureKind.NotFound => NotFound(result),
            ResultFailureKind.Conflict => Conflict(result),
            _ => BadRequest(result)
        };
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate(
        ValidateLicenseCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{licenseId:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Approve(Guid licenseId, ChangeLicenseStatusRequest request,
        CancellationToken cancellationToken) =>
        ChangeStatus(licenseId, LicenseStatusAction.Approve, request, cancellationToken);

    [HttpPost("{licenseId:guid}/suspend")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Suspend(Guid licenseId, ChangeLicenseStatusRequest request,
        CancellationToken cancellationToken) =>
        ChangeStatus(licenseId, LicenseStatusAction.Suspend, request, cancellationToken);

    [HttpPost("{licenseId:guid}/resume")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Resume(Guid licenseId, ChangeLicenseStatusRequest request,
        CancellationToken cancellationToken) =>
        ChangeStatus(licenseId, LicenseStatusAction.Resume, request, cancellationToken);

    [HttpPost("{licenseId:guid}/revoke")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Revoke(Guid licenseId, ChangeLicenseStatusRequest request,
        CancellationToken cancellationToken) =>
        ChangeStatus(licenseId, LicenseStatusAction.Revoke, request, cancellationToken);

    private async Task<IActionResult> ChangeStatus(Guid licenseId, LicenseStatusAction action,
        ChangeLicenseStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var appUserId))
            return Forbid();

        var adminProfileId = await admins.GetProfileIdByAppUserIdAsync(appUserId, cancellationToken);
        if (adminProfileId is null)
            return Forbid();

        var result = await sender.Send(new ChangeLicenseStatusCommand(
            licenseId, adminProfileId.Value, appUserId, action, request.Reason), cancellationToken);
        if (result.IsSuccess) return Ok(result);

        return result.FailureKind switch
        {
            ResultFailureKind.NotFound => NotFound(result),
            ResultFailureKind.Conflict => Conflict(result),
            _ => BadRequest(result)
        };
    }

    private async Task<IActionResult> GetDetailsResult(Guid licenseId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLicenseDetailsQuery(licenseId), cancellationToken);
        return result.IsSuccess ? Ok(result) : DetailFailure(result);
    }

    private IActionResult DetailFailure<T>(ResultDto<T> result) =>
        result.FailureKind == ResultFailureKind.NotFound ? NotFound(result) : BadRequest(result);
}
