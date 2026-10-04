using LicenseGuard.Domain.Dtos;
using MediatR;

namespace LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand;

public sealed record CreateLicenseCommand(Guid SubscriptionId, Guid IssuedByAdminId, Guid IssuedByUserId,
    int MaxActivations,
    IReadOnlyCollection<Guid>? FeatureIds = null)
    : IRequest<ResultDto<CreateLicenseResponse>>;

public sealed record CreateLicenseResponse(
    Guid LicenseId,
    string LicenseKey,
    string Signature,
    Guid ProductId,
    Guid PlanId,
    DateTime StartDate,
    DateTime ExpirationDate);
