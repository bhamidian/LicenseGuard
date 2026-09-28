using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Domain.Tests;

internal static class DomainTestData
{
    public static readonly Guid AdminId = Guid.NewGuid();
    public static DateTime Start => new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    public static DateTime End => new(2027, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    public static (Customer Customer, Product Product, Plan Plan, Feature Feature) Catalog()
    {
        var customer = new Customer(Guid.NewGuid());
        var product = new Product("suite", "Suite", "A product suite");
        var plan = product.AddPlan("Professional", "Professional plan", 50m);
        var feature = new Feature("reports", "Reports", "Reporting tools");
        product.AddFeature(feature);
        plan.AddFeature(feature);
        return (customer, product, plan, feature);
    }

    public static Subscription Subscription()
    {
        var (customer, product, plan, _) = Catalog();
        return new Subscription(customer, product, plan, Start, End, SubscriptionStatusEnum.Active);
    }

    public static License License(int maxActivations = 1, LicenseStatusEnum status = LicenseStatusEnum.ACTIVE)
    {
        var subscription = Subscription();
        var license = new License(subscription.Id, AdminId, status, Start, End, maxActivations);
        subscription.AddLicense(license);
        return license;
    }

    public static MachineId Machine(string value = "machine-01") => MachineId.Create(value);
    public static InstanceId Instance(string value = "instance-01") => InstanceId.Create(value);
}
