using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using MediatR;

namespace LicenseGuard.Application.Features.License.Queries.GetCustomerLicenses;

public sealed record GetCustomerLicensesQuery(Guid AppUserId, int Page = 1, int PageSize = 25)
    : IRequest<ResultDto<CustomerLicensesResponse>>;

public sealed record CustomerLicensesResponse(
    IReadOnlyCollection<CustomerLicenseItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record CustomerLicenseItemResponse(
    Guid LicenseId,
    string LicenseKey,
    Guid ProductId,
    Guid PlanId,
    LicenseStatusEnum Status,
    DateTime CreatedAt,
    DateTime StartDate,
    DateTime ExpirationDate,
    int MaxActivations,
    int ActiveActivations,
    int EnabledFeatureCount,
    bool AutoRenewalEnabled);
