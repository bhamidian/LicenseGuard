using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Exceptions;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.ValueObjects;

namespace LicenseGuard.Domain.Entities;

public class License : BaseEntity
{
    private const string MaxActivationsLimitCode = "max_activations";

    private License() { }

    internal License(Guid subscriptionId, Guid adminId, LicenseKey key, LicenseStatusEnum licenseStatus,
        DateTime startDate, DateTime endDate, int maxActivations = 1, Guid customerId = default,
        Guid productId = default, Guid planId = default, string policyVersion = "1")
    {
        if (subscriptionId == Guid.Empty) throw new DomainValidationException("Subscription ID cannot be empty.");
        if (adminId == Guid.Empty) throw new DomainValidationException("Admin ID cannot be empty.");
        ArgumentNullException.ThrowIfNull(key);
        if (!Enum.IsDefined(licenseStatus) || licenseStatus is LicenseStatusEnum.REVOKED or LicenseStatusEnum.EXPIRED or LicenseStatusEnum.RESUMED)
            throw new DomainValidationException("A new license must start in Pending, Active or Suspended state.");
        if (maxActivations < 1) throw new DomainValidationException("At least one activation must be allowed.");
        StartDate = EnsureUtc(startDate);
        ExpirationDate = EnsureUtc(endDate);
        IssuedExpirationDate = ExpirationDate;
        if (ExpirationDate <= StartDate) throw new DomainValidationException("Expiration must be later than start date.");

        SubscriptionId = subscriptionId;
        CustomerId = customerId;
        ProductId = productId;
        PlanId = planId;
        PolicyVersion = DomainText.Required(policyVersion, 50, nameof(policyVersion));
        AdminId = adminId;
        LicenseStatus = licenseStatus;
        Key = key;
        AutoRenewal = new AutoRenewal(false);
        var activationLimit = new LicenseLimit(Id, MaxActivationsLimitCode, maxActivations, "activations");
        activationLimit.AttachToLicense(this);
        Limits.Add(activationLimit);
        var initialHistory = new LicenseStatusHistory(Id, licenseStatus, adminId, "License created.");
        initialHistory.SetLicense(this);
        StatusHistory.Add(initialHistory);
        SetCreatedBy(adminId);
    }

    public DateTime StartDate { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid PlanId { get; private set; }
    public string PolicyVersion { get; private set; } = "1";
    public Guid AdminId { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public DateTime IssuedExpirationDate { get; private set; }
    public LicenseKey Key { get; private set; } = null!;
    public LicenseStatusEnum LicenseStatus { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Subscription Subscription { get; private set; } = null!;
    public Admin Admin { get; private set; } = null!;
    public LicenseSignature Signature { get; private set; } = null!;
    public AutoRenewal AutoRenewal { get; private set; } = new(false);
    public ICollection<AuditLog> AuditLogs { get; private set; } = new List<AuditLog>();
    public ICollection<LicenseLimit> Limits { get; private set; } = new List<LicenseLimit>();
    public ICollection<LicenseFeature> LicenseFeatures { get; private set; } = new List<LicenseFeature>();
    public ICollection<LicenseActivation> Activations { get; private set; } = new List<LicenseActivation>();
    public ICollection<LicenseStatusHistory> StatusHistory { get; private set; } = new List<LicenseStatusHistory>();

    public LicenseSigningData GetSigningData()
    {
        if (Key is null || CustomerId == Guid.Empty || ProductId == Guid.Empty || PlanId == Guid.Empty)
            throw new DomainRuleViolationException("License snapshot is incomplete and cannot be signed.");
        return new LicenseSigningData(Key.Value, CustomerId, ProductId, PlanId, StartDate, IssuedExpirationDate,
            PolicyVersion,
            LicenseFeatures.OrderBy(x => x.FeatureId).Select(x => new FeatureSigningData(x.FeatureId, x.FeatureCodeSnapshot, x.IsEnabled)).ToArray(),
            Limits.OrderBy(x => x.Code, StringComparer.Ordinal).Select(x => new LimitSigningData(x.Code, x.Value, x.Unit)).ToArray());
    }

    public void SetSignature(LicenseSignature signature)
    {
        ArgumentNullException.ThrowIfNull(signature);
        Signature = signature;
        Touch();
    }

    public void SetDescription(string? description)
    {
        Description = DomainText.Optional(description, 2000, nameof(description));
        Touch();
    }

    public void SetAutoRenewal(AutoRenewal autoRenewal)
    {
        ArgumentNullException.ThrowIfNull(autoRenewal);
        AutoRenewal = autoRenewal;
        Touch();
    }

    public LicenseLimit SetLimit(string code, decimal value, string? unit = null)
    {
        EnsureSnapshotNotSigned();
        var normalizedCode = DomainText.Required(code, 100, nameof(code)).ToLowerInvariant();
        if (normalizedCode == MaxActivationsLimitCode && (value < 1 || value != decimal.Truncate(value)))
            throw new DomainValidationException("Maximum activations must be a positive whole number.");
        var existing = Limits.SingleOrDefault(limit => limit.Code == normalizedCode);
        if (existing is not null)
        {
            existing.SetValue(value, unit);
            return existing;
        }
        var limit = new LicenseLimit(Id, normalizedCode, value, unit);
        limit.AttachToLicense(this);
        Limits.Add(limit);
        return limit;
    }

    public LicenseFeature GrantFeature(Feature feature, bool enabled = true)
    {
        EnsureSnapshotNotSigned();
        ArgumentNullException.ThrowIfNull(feature);
        var existing = LicenseFeatures.SingleOrDefault(link => link.FeatureId == feature.Id);
        if (existing is not null)
        {
            existing.SetEnabled(enabled);
            return existing;
        }
        var link = new LicenseFeature(Id, feature, enabled);
        link.AttachToLicense(this);
        LicenseFeatures.Add(link);
        return link;
    }

    public void AddAuditLog(AuditLog auditLog)
    {
        ArgumentNullException.ThrowIfNull(auditLog);
        auditLog.AttachToLicense(this);
        AuditLogs.Add(auditLog);
    }

    public LicenseActivation Activate(MachineId machineId, InstanceId instanceId, string ipAddress, DateTime? at = null)
    {
        ArgumentNullException.ThrowIfNull(machineId);
        ArgumentNullException.ThrowIfNull(instanceId);
        var instant = EnsureUtc(at ?? DateTime.UtcNow);
        var activation = new LicenseActivation(Id, machineId, instanceId, ipAddress, instant);
        return Activate(activation, instant);
    }

    public LicenseActivation Activate(LicenseActivation activation, DateTime? at = null)
    {
        ArgumentNullException.ThrowIfNull(activation);
        if (activation.LicenseId != Id)
            throw new DomainRuleViolationException("Activation belongs to another license.");
        var instant = EnsureUtc(at ?? DateTime.UtcNow);
        EnsureUsableAt(instant);
        var max = (int)(Limits.SingleOrDefault(limit => limit.Code == MaxActivationsLimitCode)?.Value ?? 1);
        if (Activations.Count(activation => activation.Status == ActivationStatusEnum.Active) >= max)
            throw new DomainRuleViolationException("The license activation limit has been reached.");
        if (Activations.Any(existing => existing.Status == ActivationStatusEnum.Active &&
            (existing.MachineId.Equals(activation.MachineId) || existing.InstanceId.Equals(activation.InstanceId))))
            throw new DomainRuleViolationException("This machine or instance is already activated.");
        activation.AttachToLicense(this);
        Activations.Add(activation);
        return activation;
    }

    public void Suspend(Guid adminId, string? reason = null) => ChangeStatus(LicenseStatusEnum.SUSPENDED, adminId, reason);
    public void Revoke(Guid adminId, string? reason = null) => ChangeStatus(LicenseStatusEnum.REVOKED, adminId, reason);

    public void Resume(Guid adminId, string? reason = null)
    {
        if (LicenseStatus != LicenseStatusEnum.SUSPENDED)
            throw new DomainRuleViolationException("Only a suspended license can be resumed.");
        EnsureNotExpired(DateTime.UtcNow);
        ChangeStatus(LicenseStatusEnum.ACTIVE, adminId, reason);
    }

    public void Expire(DateTime? at = null)
    {
        var now = EnsureUtc(at ?? DateTime.UtcNow);
        if (now < ExpirationDate) throw new DomainRuleViolationException("A license cannot expire before its expiration date.");
        if (LicenseStatus is LicenseStatusEnum.REVOKED or LicenseStatusEnum.EXPIRED) return;
        ChangeStatus(LicenseStatusEnum.EXPIRED, null, "License validity period ended.", now);
    }

    public void Renew(DateTime newExpirationDate, decimal amount, Guid renewedByUserId, DateTime? renewedAt = null)
    {
        if (LicenseStatus == LicenseStatusEnum.REVOKED)
            throw new DomainRuleViolationException("A revoked license cannot be renewed.");
        if (renewedByUserId == Guid.Empty) throw new DomainValidationException("Renewing user ID cannot be empty.");
        if (amount < 0) throw new DomainValidationException("Renewal amount cannot be negative.");
        var newExpirationUtc = EnsureUtc(newExpirationDate);
        if (newExpirationUtc <= ExpirationDate)
            throw new DomainValidationException("Renewal expiration must extend the current expiration.");
        ExpirationDate = newExpirationUtc;
        IssuedExpirationDate = newExpirationUtc;
        if (LicenseStatus == LicenseStatusEnum.EXPIRED)
        {
            LicenseStatus = LicenseStatusEnum.ACTIVE;
        }
        var renewalHistory = new LicenseStatusHistory(Id, LicenseStatus, renewedByUserId, "License renewed.", renewedAt);
        renewalHistory.SetLicense(this);
        StatusHistory.Add(renewalHistory);
        Touch(renewedAt);
    }

    public bool IsValidAt(DateTime at)
    {
        var instant = EnsureUtc(at);
        return LicenseStatus == LicenseStatusEnum.ACTIVE
            && instant >= StartDate
            && instant < ExpirationDate
            && Subscription is not null
            && Subscription.CurrentLicenseId == Id
            && Subscription.IsActiveAt(instant);
    }

    internal void SetSubscription(Subscription subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        if (subscription.Id != SubscriptionId)
            throw new DomainRuleViolationException("License belongs to another subscription.");
        Subscription = subscription;
    }

    internal void AddSnapshotFeature(Feature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);
        if (LicenseFeatures.Any(x => x.FeatureId == feature.Id)) return;
        var link = new LicenseFeature(Id, feature);
        link.AttachToLicense(this);
        LicenseFeatures.Add(link);
    }

    private void EnsureSnapshotNotSigned()
    {
        if (Signature is not null)
            throw new DomainRuleViolationException("Signed license entitlements are immutable.");
    }

    private void EnsureUsableAt(DateTime at)
    {
        EnsureNotExpired(at);
        if (LicenseStatus != LicenseStatusEnum.ACTIVE)
            throw new DomainRuleViolationException("Only an active license can be activated.");
        if (!IsValidAt(at)) throw new DomainRuleViolationException("License is outside its validity period.");
    }

    private void EnsureNotExpired(DateTime at)
    {
        if (EnsureUtc(at) >= ExpirationDate || LicenseStatus == LicenseStatusEnum.EXPIRED)
            throw new DomainRuleViolationException("License has expired.");
    }

    private void ChangeStatus(LicenseStatusEnum newStatus, Guid? adminId, string? reason, DateTime? at = null)
    {
        if (adminId == Guid.Empty) throw new DomainValidationException("Admin ID cannot be empty when supplied.");
        if (LicenseStatus is LicenseStatusEnum.REVOKED or LicenseStatusEnum.EXPIRED)
            throw new DomainRuleViolationException("A revoked or expired license cannot change status.");
        if (LicenseStatus == newStatus) return;
        var allowed = newStatus switch
        {
            LicenseStatusEnum.SUSPENDED => LicenseStatus is LicenseStatusEnum.ACTIVE or LicenseStatusEnum.PENDING,
            LicenseStatusEnum.ACTIVE => LicenseStatus is LicenseStatusEnum.PENDING or LicenseStatusEnum.SUSPENDED,
            LicenseStatusEnum.REVOKED => LicenseStatus is LicenseStatusEnum.ACTIVE or LicenseStatusEnum.PENDING or LicenseStatusEnum.SUSPENDED,
            LicenseStatusEnum.EXPIRED => LicenseStatus is LicenseStatusEnum.ACTIVE or LicenseStatusEnum.PENDING or LicenseStatusEnum.SUSPENDED,
            _ => false
        };
        if (!allowed) throw new DomainRuleViolationException($"Transition from {LicenseStatus} to {newStatus} is not allowed.");
        LicenseStatus = newStatus;
        var history = new LicenseStatusHistory(Id, newStatus, adminId, reason, at);
        history.SetLicense(this);
        StatusHistory.Add(history);
        Touch(at);
    }
}
