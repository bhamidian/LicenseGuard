namespace LicenseGuard.Domain.Records
{
    public sealed record LicenseSigningData(
        string LicenseKey,
        Guid CustomerId,
        Guid ProductId,
        Guid PlanId,
        DateTime StartDate,
        DateTime ExpirationDate,
        string PolicyVersion,
        IReadOnlyList<FeatureSigningData> Features,
        IReadOnlyList<LimitSigningData> Limits
    );

    public sealed record FeatureSigningData(Guid FeatureId, string Code, bool IsEnabled);
    public sealed record LimitSigningData(string Code, decimal Value, string? Unit);
}
