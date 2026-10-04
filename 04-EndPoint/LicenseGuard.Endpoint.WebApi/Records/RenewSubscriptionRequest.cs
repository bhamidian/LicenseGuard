namespace LicenseGuard.Endpoint.WebApi.Records;

public sealed record RenewSubscriptionRequest(decimal Amount, DateTime NewExpirationDate);
