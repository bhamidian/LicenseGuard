using LicenseGuard.Endpoint.WebApi.Controllers;
using LicenseGuard.Endpoint.WebApi.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace LicenseGuard.Endpoint.WebApi.Identity;

public sealed class IdentityRateLimitConfiguration(IOptions<RateLimitOptions> rateLimitOptions)
    : IConfigureOptions<RateLimiterOptions>, IConfigureOptions<MvcOptions>
{
    private readonly RateLimitOptions _options = rateLimitOptions.Value;

    public void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = _options.RejectionStatusCode;
        options.AddPolicy(_options.LicenseClientPolicyName, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? _options.UnknownClientPartitionKey,
                _ => CreateWindowOptions(_options.LicenseClientPermitLimit)));
        options.AddPolicy(_options.AuthRequestCodePolicyName, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? _options.UnknownClientPartitionKey,
                _ => CreateWindowOptions(_options.AuthRequestCodePermitLimit)));
        options.AddPolicy(_options.AuthVerifyCodePolicyName, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? _options.UnknownClientPartitionKey,
                _ => CreateWindowOptions(_options.AuthVerifyCodePermitLimit)));
    }

    public void Configure(MvcOptions options) =>
        options.Conventions.Add(new RateLimitPolicyConvention(_options));

    private FixedWindowRateLimiterOptions CreateWindowOptions(int permitLimit) => new()
    {
        PermitLimit = permitLimit,
        Window = TimeSpan.FromMinutes(_options.WindowMinutes),
        QueueLimit = _options.QueueLimit,
        AutoReplenishment = _options.AutoReplenishment
    };

    private sealed class RateLimitPolicyConvention(RateLimitOptions options) : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var action in application.Controllers.SelectMany(controller => controller.Actions))
            {
                var policyName = (action.Controller.ControllerName, action.ActionName) switch
                {
                    ("Auth", nameof(AuthController.RequestCode)) => options.AuthRequestCodePolicyName,
                    ("Auth", nameof(AuthController.VerifyCode)) => options.AuthVerifyCodePolicyName,
                    ("License", nameof(LicenseController.Activate)) => options.LicenseClientPolicyName,
                    ("License", nameof(LicenseController.Validate)) => options.LicenseClientPolicyName,
                    _ => null
                };

                if (!string.IsNullOrWhiteSpace(policyName))
                {
                    foreach (var selector in action.Selectors)
                        selector.EndpointMetadata.Add(new EnableRateLimitingAttribute(policyName));
                }
            }
        }
    }
}
