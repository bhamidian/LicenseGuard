namespace LicenseGuard.Domain.LicenseAgg.Records
{
    public sealed record LicenseSigningData(
        string LicenseKey,
        Guid ProductId,
        Guid PlanId,
        DateTime StartDate,
        DateTime ExpirationDate
    );
}