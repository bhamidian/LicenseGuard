using LicenseGuard.Domain.Dtos;
using LicenseGuard.Application.Features.Auth.Records;
using LicenseGuard.Infrastructure.JwtService.Contracts;
using MediatR;

namespace LicenseGuard.Application.Features.Auth.Commands.VerifyLoginCode;

public sealed record VerifyLoginCodeCommand(LoginCodeChannel Channel, string Contact, string Code)
    : IRequest<ResultDto<VerifyLoginCodeResponse>>;

public sealed record VerifyLoginCodeResponse(Guid UserId, IReadOnlyCollection<string> Roles, JwtTokenResponse Token);
