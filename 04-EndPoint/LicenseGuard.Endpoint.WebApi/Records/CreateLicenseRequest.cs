namespace LicenseGuard.Endpoint.WebApi.Records;

public sealed record CreateLicenseRequest(
    Guid SubscriptionId,
    int MaxActivations,
    IReadOnlyCollection<Guid>? FeatureIds = null);
