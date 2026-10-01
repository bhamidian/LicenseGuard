namespace LicenseGuard.Endpoint.WebApi.Configuration;

public sealed class IdentityPolicyOptions
{
    public const string SectionName = "Identity";

    public bool RequireUniqueEmail { get; set; }
    public bool LockoutEnabledForNewUsers { get; set; }
    public int MaxFailedAccessAttempts { get; set; }
    public int LockoutMinutes { get; set; }
}
