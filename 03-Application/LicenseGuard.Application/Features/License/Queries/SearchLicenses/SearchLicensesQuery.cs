using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Enums;
using MediatR;

namespace LicenseGuard.Application.Features.License.Queries.SearchLicenses;

public sealed record SearchLicensesQuery(
    string? LicenseKey,
    Guid? CustomerId,
    Guid? ProductId,
    Guid? PlanId,
    LicenseStatusEnum? Status,
    int? ExpiresWithinDays,
    int Page = 1,
    int PageSize = 25) : IRequest<ResultDto<SearchLicensesResponse>>;

public sealed record SearchLicensesResponse(
    IReadOnlyCollection<LicenseListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record LicenseListItemResponse(
    Guid LicenseId,
    string MaskedLicenseKey,
    Guid CustomerId,
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
