using LicenseGuard.Application.Features.License.Commands.ActivateLicense;
using LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand;
using LicenseGuard.Application.Features.License.Commands.ValidateLicense;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Endpoint.WebApi.Records;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LicenseGuard.Endpoint.WebApi.Controllers;

[ApiController]
[Route("api/licenses")]
public sealed class LicenseController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateLicenseCommand command,
        CancellationToken cancellationToken)
    {
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
}
