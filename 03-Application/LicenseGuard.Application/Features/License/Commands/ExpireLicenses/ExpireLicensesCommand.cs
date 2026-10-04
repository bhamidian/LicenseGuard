using LicenseGuard.Domain.Dtos;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.ExpireLicenses;

public sealed record ExpireLicensesCommand(int BatchSize = 100) : IRequest<ResultDto<int>>;
