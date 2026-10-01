using LicenseGuard.Application.Features.Auth.Commands.RequestLoginCode;
using LicenseGuard.Application.Features.Auth.Commands.VerifyLoginCode;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Endpoint.WebApi.Records;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LicenseGuard.Endpoint.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode(
        RequestLoginCodeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RequestLoginCodeCommand(request.Channel, request.Contact), cancellationToken);
        if (result.IsSuccess) return Accepted(value: result.Data);

        return result.FailureKind == ResultFailureKind.Unavailable
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, result)
            : BadRequest(result);
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode(
        VerifyLoginCodeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new VerifyLoginCodeCommand(request.Channel, request.Contact, request.Code), cancellationToken);
        if (result.IsSuccess) return Ok(result.Data);

        return result.FailureKind == ResultFailureKind.Unauthorized
            ? Unauthorized(result)
            : BadRequest(result);
    }
}
