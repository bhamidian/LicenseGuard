namespace LicenseGuard.Domain.Records;

public sealed record CreatePendingAppUserRecord(string UserName, string? Email, string? PhoneNumber);
