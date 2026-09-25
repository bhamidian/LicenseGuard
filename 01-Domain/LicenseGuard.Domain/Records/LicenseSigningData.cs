namespace LicenseGuard.Domain.Records
{
    public sealed record LicenseSigningData(
        string LicenseKey,
        Guid ProductId,
        Guid PlanId,
        DateTime StartDate,
        DateTime ExpirationDate
    );
}