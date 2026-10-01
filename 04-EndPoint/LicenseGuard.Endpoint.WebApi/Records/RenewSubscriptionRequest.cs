namespace LicenseGuard.Endpoint.WebApi.Records;

public sealed record RenewSubscriptionRequest(Guid RenewedByUserId, decimal Amount, DateTime NewExpirationDate);
