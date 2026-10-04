namespace LicenseGuard.Endpoint.WebApi.Records;

public sealed record DeactivateLicenseActivationRequest(string? Reason = null);
