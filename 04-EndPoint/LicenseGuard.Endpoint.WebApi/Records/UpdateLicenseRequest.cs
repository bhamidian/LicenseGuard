using LicenseGuard.Domain.Enums;

namespace LicenseGuard.Endpoint.WebApi.Records;

public sealed record UpdateLicenseRequest(
    string? Description = null,
    bool? AutoRenewalEnabled = null,
    AutoRenwalPlanEnum? AutoRenewalPlan = null,
    int? MaxActivations = null,
    IReadOnlyCollection<Guid>? FeatureIds = null,
    IReadOnlyCollection<UpdateLicenseLimitRequest>? Limits = null,
    string? Reason = null);

public sealed record UpdateLicenseLimitRequest(string Code, decimal Value, string? Unit = null);
