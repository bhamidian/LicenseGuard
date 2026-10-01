namespace LicenseGuard.Endpoint.WebApi.Records;

public sealed record ActivateLicenseRequest(string LicenseKey, string MachineId, string InstanceId);
