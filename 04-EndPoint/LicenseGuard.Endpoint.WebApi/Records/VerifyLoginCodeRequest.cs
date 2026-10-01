using LicenseGuard.Application.Features.Auth.Records;

namespace LicenseGuard.Endpoint.WebApi.Records;

public sealed record VerifyLoginCodeRequest(LoginCodeChannel Channel, string Contact, string Code);
