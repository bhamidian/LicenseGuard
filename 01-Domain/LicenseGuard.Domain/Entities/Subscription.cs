using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Exceptions;

namespace LicenseGuard.Domain.Entities;

public class Subscription : BaseEntity
{
    private Subscription() { }

    public Subscription(Customer customer, Product product, Plan plan, DateTime startDate, DateTime endDate,
        SubscriptionStatusEnum status = SubscriptionStatusEnum.Trial, string? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(plan);
        if (customer.Id == Guid.Empty || product.Id == Guid.Empty || plan.Id == Guid.Empty)
            throw new DomainValidationException("Subscription references must have valid IDs.");
        if (plan.ProductId != product.Id)
            throw new DomainRuleViolationException("Subscription plan must belong to the selected product.");
        StartDate = EnsureUtc(startDate);
        EndDate = EnsureUtc(endDate);
        if (EndDate <= StartDate) throw new DomainValidationException("Subscription end date must follow its start date.");
        if (!Enum.IsDefined(status)) throw new DomainValidationException("Subscription status is invalid.");
        Customer = customer;
        CustomerId = customer.Id;
        Product = product;
        ProductId = product.Id;
        Plan = plan;
        PlanId = plan.Id;
        Status = status;
        MetaData = DomainText.Optional(metadata, 4000, nameof(metadata));
        customer.AddSubscription(this);
        plan.Subscriptions.Add(this);
    }

    public Guid PlanId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime EndDate { get; private set; }
    public string? MetaData { get; private set; }
    public DateTime StartDate { get; private set; }
    public Plan Plan { get; private set; } = null!;
    public DateTime? CancelledAt { get; private set; }
    public Guid? CurrentLicenseId { get; private set; }
    public License? CurrentLicense { get; private set; }
    public Product Product { get; private set; } = null!;
    public Customer Customer { get; private set; } = null!;
    public SubscriptionStatusEnum Status { get; private set; }
    public ICollection<License> Licenses { get; private set; } = new List<License>();
    public ICollection<SubscriptionRenewal> Renewals { get; private set; } = new List<SubscriptionRenewal>();

    public void SetCurrentLicense(License license)
    {
        ArgumentNullException.ThrowIfNull(license);
        if (license.SubscriptionId != Id)
            throw new DomainRuleViolationException("The current license must belong to this subscription.");
        if (license.Id == Guid.Empty)
            throw new DomainValidationException("The current license must have a valid ID.");
        if (!Licenses.Any(item => item.Id == license.Id)) Licenses.Add(license);
        license.SetSubscription(this);
        CurrentLicense = license;
        CurrentLicenseId = license.Id;
        Touch();
    }

    public void ClearCurrentLicense()
    {
        CurrentLicense = null;
        CurrentLicenseId = null;
        Touch();
    }

    public void AddLicense(License license, bool setAsCurrent = true)
    {
        ArgumentNullException.ThrowIfNull(license);
        if (license.SubscriptionId != Id)
            throw new DomainRuleViolationException("License belongs to another subscription.");
        if (Licenses.Any(item => item.Id == license.Id))
            throw new DomainRuleViolationException("License is already linked to this subscription.");
        Licenses.Add(license);
        license.SetSubscription(this);
        if (setAsCurrent) SetCurrentLicense(license);
    }

    public void SetMetadata(string? metadata)
    {
        MetaData = DomainText.Optional(metadata, 4000, nameof(metadata));
        Touch();
    }

    public void ChangePlan(Plan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.ProductId != ProductId)
            throw new DomainRuleViolationException("A subscription plan must belong to the same product.");
        Plan = plan;
        PlanId = plan.Id;
        Touch();
    }

    public void Renew(SubscriptionRenewal renewal, Guid renewedByUserId)
    {
        ArgumentNullException.ThrowIfNull(renewal);
        if (renewal.SubscriptionId != Id)
            throw new DomainRuleViolationException("Renewal belongs to another subscription.");
        if (renewal.PreviousExpirationDate != EndDate)
            throw new DomainRuleViolationException("Renewal previous expiration does not match the subscription.");
        if (Status == SubscriptionStatusEnum.Canceled)
            throw new DomainRuleViolationException("A canceled subscription cannot be renewed.");
        if (CurrentLicense is not null)
            CurrentLicense.Renew(renewal.NewExpirationDate, renewal.Amount, renewedByUserId, renewal.RenewedAt);
        Renewals.Add(renewal);
        EndDate = renewal.NewExpirationDate;
        Status = SubscriptionStatusEnum.Active;
        Touch(renewal.RenewedAt);
    }

    public void Cancel(DateTime? at = null)
    {
        if (Status == SubscriptionStatusEnum.Canceled) return;
        if (Status == SubscriptionStatusEnum.Expired)
            throw new DomainRuleViolationException("An expired subscription cannot be canceled.");
        CancelledAt = EnsureUtc(at ?? DateTime.UtcNow);
        Status = SubscriptionStatusEnum.Canceled;
        Touch(CancelledAt);
    }

    public void Expire(DateTime? at = null)
    {
        var instant = EnsureUtc(at ?? DateTime.UtcNow);
        if (instant < EndDate) throw new DomainRuleViolationException("Subscription cannot expire before its end date.");
        if (Status == SubscriptionStatusEnum.Canceled) return;
        Status = SubscriptionStatusEnum.Expired;
        Touch(instant);
    }

    public bool IsActiveAt(DateTime at)
    {
        var instant = EnsureUtc(at);
        return Status is SubscriptionStatusEnum.Trial or SubscriptionStatusEnum.Active or SubscriptionStatusEnum.GracePeriod
            && instant >= StartDate && instant < EndDate;
    }
}
