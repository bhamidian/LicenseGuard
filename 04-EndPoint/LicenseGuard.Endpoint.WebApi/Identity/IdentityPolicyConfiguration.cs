using LicenseGuard.Endpoint.WebApi.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LicenseGuard.Endpoint.WebApi.Identity;

public sealed class IdentityPolicyConfiguration(IOptions<IdentityPolicyOptions> policyOptions)
    : IConfigureOptions<IdentityOptions>
{
    public void Configure(IdentityOptions options)
    {
        var policy = policyOptions.Value;
        options.User.RequireUniqueEmail = policy.RequireUniqueEmail;
        options.Lockout.AllowedForNewUsers = policy.LockoutEnabledForNewUsers;
        options.Lockout.MaxFailedAccessAttempts = policy.MaxFailedAccessAttempts;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(policy.LockoutMinutes);
    }
}
