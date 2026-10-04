namespace LicenseGuard.Domain.Records
{
    public sealed record FeatureSigningData(Guid FeatureId, string Code, bool IsEnabled);
}
