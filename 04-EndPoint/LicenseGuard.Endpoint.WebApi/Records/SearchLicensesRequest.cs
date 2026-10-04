using LicenseGuard.Domain.Enums;

namespace LicenseGuard.Endpoint.WebApi.Records;

public sealed record SearchLicensesRequest(
    string? LicenseKey,
    Guid? CustomerId,
    Guid? ProductId,
    Guid? PlanId,
    LicenseStatusEnum? Status,
    int? ExpiresWithinDays,
    int Page = 1,
    int PageSize = 25);
