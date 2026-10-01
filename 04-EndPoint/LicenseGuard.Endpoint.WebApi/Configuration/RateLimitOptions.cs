namespace LicenseGuard.Endpoint.WebApi.Configuration;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimits";

    public string LicenseClientPolicyName { get; set; } = string.Empty;
    public string AuthRequestCodePolicyName { get; set; } = string.Empty;
    public string AuthVerifyCodePolicyName { get; set; } = string.Empty;
    public int LicenseClientPermitLimit { get; set; }
    public int AuthRequestCodePermitLimit { get; set; }
    public int AuthVerifyCodePermitLimit { get; set; }
    public int WindowMinutes { get; set; }
    public int QueueLimit { get; set; }
    public int RejectionStatusCode { get; set; }
    public bool AutoReplenishment { get; set; }
    public string UnknownClientPartitionKey { get; set; } = string.Empty;
}
