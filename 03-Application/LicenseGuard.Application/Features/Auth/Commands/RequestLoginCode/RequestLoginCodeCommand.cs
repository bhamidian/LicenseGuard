using LicenseGuard.Domain.Dtos;
using LicenseGuard.Application.Features.Auth.Records;
using MediatR;

namespace LicenseGuard.Application.Features.Auth.Commands.RequestLoginCode;

public sealed record RequestLoginCodeCommand(LoginCodeChannel Channel, string Contact)
    : IRequest<ResultDto<RequestLoginCodeResponse>>;

public sealed record RequestLoginCodeResponse(string Message);
