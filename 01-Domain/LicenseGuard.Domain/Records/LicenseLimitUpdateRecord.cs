namespace LicenseGuard.Domain.Records;

public sealed record LicenseLimitUpdateRecord(string Code, decimal Value, string? Unit = null);
