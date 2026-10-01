using LicenseGuard.Application.Features.Auth.Records;

namespace LicenseGuard.Endpoint.WebApi.Records;

public sealed record RequestLoginCodeRequest(LoginCodeChannel Channel, string Contact);
