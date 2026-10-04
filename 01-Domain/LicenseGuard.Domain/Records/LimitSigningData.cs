namespace LicenseGuard.Domain.Records
{
    public sealed record LimitSigningData(string Code, decimal Value, string? Unit);
}
