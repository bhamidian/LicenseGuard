using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Exceptions;
using Xunit;

namespace LicenseGuard.Domain.Tests;

public sealed class EntityBehaviorTests
{
    [Fact]
    public void BaseEntity_soft_delete_and_restore_keep_lifecycle_consistent()
    {
        var product = new Product("app", "App", "desc");
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.True(product.IsActive);
        var when = DomainTestData.End;
        product.SoftDelete(when);
        Assert.True(product.IsDeleted);
        Assert.False(product.IsActive);
        Assert.Equal(when, product.DeletedAt);
        Assert.Throws<DomainRuleViolationException>(() => product.Reactivate());
        product.Restore();
        Assert.True(product.IsActive);
        Assert.False(product.IsDeleted);
        Assert.Null(product.DeletedAt);
    }

    [Fact]
    public void BaseEntity_requires_creator_id_when_set()
    {
        var product = new Product("app", "App", "desc");
        Assert.Throws<DomainValidationException>(() => product.SetCreatedBy(Guid.Empty));
        product.SetCreatedBy(DomainTestData.AdminId);
        Assert.Equal(DomainTestData.AdminId, product.CreatedBy);
    }

    [Fact]
    public void User_admin_and_customer_require_application_user_ids()
    {
        Assert.Throws<DomainValidationException>(() => new Admin(Guid.Empty));
        Assert.Throws<DomainValidationException>(() => new Customer(Guid.Empty));
        var admin = new Admin(Guid.NewGuid());
        var customer = new Customer(Guid.NewGuid());
        Assert.NotEqual(Guid.Empty, admin.AppUserId);
        Assert.NotEqual(Guid.Empty, customer.AppUserId);
        Assert.Throws<DomainValidationException>(() => customer.SetAppUserId(Guid.Empty));
    }

    [Fact]
    public void AppUser_validates_role_and_associated_user()
    {
        Assert.Throws<DomainValidationException>(() => new AppUser(" ", Guid.NewGuid()));
        Assert.Throws<DomainValidationException>(() => new AppUser("admin", Guid.Empty));
        var role = new AppUser(" licensing admin ", Guid.NewGuid());
        Assert.Equal("licensing admin", role.Name);
        Assert.Equal("LICENSING ADMIN", role.NormalizedName);
        Assert.NotEqual(Guid.Empty, role.Id);
        role.RenameRole("operator");
        Assert.Equal("OPERATOR", role.NormalizedName);
    }

    [Fact]
    public void Product_updates_and_adds_plan_and_feature_once()
    {
        var product = new Product(" app ", " App ", " Description ");
        var feature = new Feature("reports", "Reports", "Reporting");
        var link = product.AddFeature(feature);
        Assert.Same(product, link.Product);
        Assert.Contains(link, feature.ProductFeatures);
        Assert.Throws<DomainRuleViolationException>(() => product.AddFeature(feature));
        var plan = product.AddPlan("Pro", "Paid plan", 10m);
        Assert.Same(product, plan.Product);
        Assert.Contains(plan, product.Plans);
        Assert.Throws<DomainValidationException>(() => product.SetDetails("", "Name", ""));
        product.SetDetails("next", "Next", "Updated");
        Assert.Equal("NEXT", product.Code);
        Assert.Throws<DomainValidationException>(() => product.AddPlan("Free", "", -1m));
    }

    [Fact]
    public void Feature_and_plan_validate_product_feature_relationship()
    {
        var product = new Product("app", "App", "");
        var plan = product.AddPlan("Pro", "", 1m);
        var feature = new Feature("reports", "Reports", "");
        Assert.Throws<DomainRuleViolationException>(() => plan.AddFeature(feature));
        product.AddFeature(feature);
        var link = feature.AddToPlan(plan);
        Assert.Contains(link, plan.PlanFeatures);
        Assert.Contains(link, feature.PlanFeatures);
        Assert.Throws<DomainRuleViolationException>(() => plan.AddFeature(feature));
        Assert.Throws<DomainValidationException>(() => feature.SetDetails("", "", ""));
    }

    [Fact]
    public void Feature_links_reject_null_references()
    {
        var product = new Product("app", "App", "");
        var feature = new Feature("reports", "Reports", "");
        Assert.Throws<ArgumentNullException>(() => new ProductFeature(product, null!));
        Assert.Throws<ArgumentNullException>(() => new PlanFeature(null!, feature));
    }

    [Fact]
    public void Plan_rejects_invalid_price_and_cross_product_feature()
    {
        var product = new Product("app", "App", "");
        Assert.Throws<DomainValidationException>(() => new Plan(Guid.NewGuid(), "Pro", "", -1));
        var plan = product.AddPlan("Pro", "", 0);
        var feature = new Feature("reports", "Reports", "");
        var otherProduct = new Product("other", "Other", "");
        otherProduct.AddFeature(feature);
        Assert.Throws<DomainRuleViolationException>(() => plan.AddFeature(feature));
        plan.SetDetails("Starter", "Updated", 9m);
        Assert.Equal(9m, plan.Price);
    }

    [Fact]
    public void Customer_tracks_only_its_own_subscriptions()
    {
        var (customer, product, plan, _) = DomainTestData.Catalog();
        var subscription = new Subscription(customer, product, plan, DomainTestData.Start, DomainTestData.End);
        Assert.Contains(subscription, customer.Subscriptions);
        var otherCustomer = new Customer(Guid.NewGuid());
        Assert.Throws<DomainRuleViolationException>(() => otherCustomer.AddSubscription(subscription));
    }

    [Fact]
    public void Subscription_enforces_plan_product_current_license_and_metadata()
    {
        var (customer, product, plan, _) = DomainTestData.Catalog();
        var otherProduct = new Product("other", "Other", "");
        var otherPlan = otherProduct.AddPlan("Pro", "", 1);
        Assert.Throws<DomainRuleViolationException>(() => new Subscription(customer, product, otherPlan, DomainTestData.Start, DomainTestData.End));
        Assert.Throws<DomainValidationException>(() => new Subscription(customer, product, plan, DomainTestData.End, DomainTestData.Start));
        var subscription = new Subscription(customer, product, plan, DomainTestData.Start, DomainTestData.End, metadata: " meta ");
        Assert.Equal("meta", subscription.MetaData);
        var license = new License(subscription.Id, DomainTestData.AdminId, LicenseStatusEnum.ACTIVE, DomainTestData.Start, DomainTestData.End);
        subscription.AddLicense(license);
        Assert.Same(license, subscription.CurrentLicense);
        Assert.Equal(license.Id, subscription.CurrentLicenseId);
        subscription.ClearCurrentLicense();
        Assert.Null(subscription.CurrentLicenseId);
        Assert.Throws<DomainRuleViolationException>(() => subscription.SetCurrentLicense(new License(Guid.NewGuid(), DomainTestData.AdminId, LicenseStatusEnum.ACTIVE, DomainTestData.Start, DomainTestData.End)));
        subscription.SetMetadata("updated");
        Assert.Equal("updated", subscription.MetaData);
    }

    [Fact]
    public void Subscription_renews_cancels_expires_and_checks_active_window()
    {
        var subscription = DomainTestData.Subscription();
        Assert.True(subscription.IsActiveAt(DomainTestData.Start.AddDays(1)));
        Assert.False(subscription.IsActiveAt(DomainTestData.End));
        var renewal = new SubscriptionRenewal(subscription.Id, 25m, DomainTestData.End, DomainTestData.End.AddYears(1), DomainTestData.End.AddDays(-1));
        subscription.Renew(renewal, Guid.NewGuid());
        Assert.Equal(renewal.NewExpirationDate, subscription.EndDate);
        Assert.Contains(renewal, subscription.Renewals);
        subscription.Cancel(DomainTestData.End);
        Assert.Equal(SubscriptionStatusEnum.Canceled, subscription.Status);
        Assert.False(subscription.IsActiveAt(DomainTestData.End.AddDays(1)));
        Assert.Throws<DomainRuleViolationException>(() => subscription.Renew(new SubscriptionRenewal(subscription.Id, 1, subscription.EndDate, subscription.EndDate.AddDays(1)), Guid.NewGuid()));
    }

    [Fact]
    public void Subscription_expires_only_after_end_date()
    {
        var subscription = DomainTestData.Subscription();
        Assert.Throws<DomainRuleViolationException>(() => subscription.Expire(DomainTestData.Start));
        subscription.Expire(DomainTestData.End);
        Assert.Equal(SubscriptionStatusEnum.Expired, subscription.Status);
    }

    [Fact]
    public void Subscription_renewal_updates_its_current_license_in_the_same_operation()
    {
        var (customer, product, plan, _) = DomainTestData.Catalog();
        var subscription = new Subscription(customer, product, plan, DomainTestData.Start, DomainTestData.End, SubscriptionStatusEnum.Active);
        var license = new License(subscription.Id, DomainTestData.AdminId, LicenseStatusEnum.ACTIVE, DomainTestData.Start, DomainTestData.End);
        subscription.AddLicense(license);
        var newEnd = DomainTestData.End.AddMonths(6);
        var renewal = new SubscriptionRenewal(subscription.Id, 100m, DomainTestData.End, newEnd);
        subscription.Renew(renewal, Guid.NewGuid());
        Assert.Equal(newEnd, subscription.EndDate);
        Assert.Equal(newEnd, license.ExpirationDate);
        Assert.Same(subscription, license.Subscription);
        Assert.Equal(LicenseStatusEnum.ACTIVE, license.LicenseStatus);
    }

    [Fact]
    public void License_initializes_secure_key_and_activation_limit()
    {
        var license = DomainTestData.License(3);
        Assert.Equal(64, license.Key.Value.Length);
        Assert.Equal(3m, Assert.Single(license.Limits).Value);
        Assert.Single(license.StatusHistory);
        Assert.Equal(DomainTestData.AdminId, license.CreatedBy);
        Assert.Throws<DomainValidationException>(() => DomainTestData.License(0));
        Assert.Throws<DomainValidationException>(() => new License(Guid.Empty, DomainTestData.AdminId, LicenseStatusEnum.ACTIVE, DomainTestData.Start, DomainTestData.End));
    }

    [Fact]
    public void License_suspension_resume_revoke_and_expiry_follow_allowed_transitions()
    {
        var license = DomainTestData.License();
        license.Suspend(DomainTestData.AdminId, "review");
        Assert.Equal(LicenseStatusEnum.SUSPENDED, license.LicenseStatus);
        license.Resume(DomainTestData.AdminId);
        Assert.Equal(LicenseStatusEnum.ACTIVE, license.LicenseStatus);
        license.Revoke(DomainTestData.AdminId, "security");
        Assert.Equal(LicenseStatusEnum.REVOKED, license.LicenseStatus);
        Assert.Throws<DomainRuleViolationException>(() => license.Resume(DomainTestData.AdminId));
        Assert.Equal(4, license.StatusHistory.Count);
        var expired = DomainTestData.License();
        Assert.Throws<DomainRuleViolationException>(() => expired.Expire(DomainTestData.Start));
        expired.Expire(DomainTestData.End);
        Assert.Equal(LicenseStatusEnum.EXPIRED, expired.LicenseStatus);
        Assert.False(expired.IsValidAt(DomainTestData.End));
    }

    [Fact]
    public void License_enforces_activation_quota_and_allows_capacity_after_deactivation()
    {
        var license = DomainTestData.License(1);
        var first = license.Activate(DomainTestData.Machine(), DomainTestData.Instance(), "127.0.0.1", DomainTestData.Start.AddDays(1));
        Assert.Throws<DomainRuleViolationException>(() => license.Activate(DomainTestData.Machine("machine-02"), DomainTestData.Instance("instance-02"), "127.0.0.2", DomainTestData.Start.AddDays(1)));
        first.Deactivate(DomainTestData.Start.AddDays(2));
        var second = license.Activate(DomainTestData.Machine("machine-02"), DomainTestData.Instance("instance-02"), "127.0.0.2", DomainTestData.Start.AddDays(3));
        Assert.Equal(ActivationStatusEnum.Active, second.Status);
    }

    [Fact]
    public void License_prevents_duplicate_instance_and_non_active_activation()
    {
        var license = DomainTestData.License(2);
        license.Activate(DomainTestData.Machine(), DomainTestData.Instance(), "127.0.0.1", DomainTestData.Start.AddDays(1));
        Assert.Throws<DomainRuleViolationException>(() => license.Activate(DomainTestData.Machine("another"), DomainTestData.Instance(), "127.0.0.2", DomainTestData.Start.AddDays(1)));
        license.Suspend(DomainTestData.AdminId);
        Assert.Throws<DomainRuleViolationException>(() => license.Activate(DomainTestData.Machine("new"), DomainTestData.Instance("new"), "127.0.0.3", DomainTestData.Start.AddDays(1)));
        Assert.Throws<DomainValidationException>(() => license.SetLimit("max_activations", 1.5m));
    }

    [Fact]
    public void License_validity_requires_its_subscription_to_be_active_and_to_point_to_it_as_current()
    {
        var license = DomainTestData.License();
        Assert.True(license.IsValidAt(DomainTestData.Start.AddDays(1)));
        license.Subscription.Cancel(DomainTestData.Start.AddDays(2));
        Assert.False(license.IsValidAt(DomainTestData.Start.AddDays(3)));
    }

    [Fact]
    public void License_updates_limits_features_audit_description_signature_and_signing_data()
    {
        var license = DomainTestData.License();
        var feature = new Feature("reports", "Reports", "");
        var grant = license.GrantFeature(feature);
        Assert.True(grant.IsEnabled);
        Assert.Same(grant, license.GrantFeature(feature, false));
        Assert.False(grant.IsEnabled);
        var limit = license.SetLimit("users", 12, "users");
        Assert.Same(limit, license.SetLimit("USERS", 20, "users"));
        Assert.Equal(20m, limit.Value);
        Assert.Same(license, limit.License);
        Assert.Same(license, grant.License);
        var audit = new AuditLog("license.updated", "License", license.Id.ToString(), true, licenseId: license.Id);
        license.AddAuditLog(audit);
        Assert.Contains(audit, license.AuditLogs);
        Assert.Same(license, audit.License);
        license.SetDescription("memo");
        Assert.Equal("memo", license.Description);
        license.SetSignature(LicenseGuard.Domain.ValueObjects.LicenseSignature.Create("signature"));
        var (customer, product, plan, _) = DomainTestData.Catalog();
        var subscription = new Subscription(customer, product, plan, DomainTestData.Start, DomainTestData.End);
        var linked = new License(subscription.Id, DomainTestData.AdminId, LicenseStatusEnum.ACTIVE, DomainTestData.Start, DomainTestData.End);
        subscription.AddLicense(linked);
        Assert.Equal(product.Id, linked.GetSigningData().ProductId);
    }

    [Fact]
    public void License_renewal_extends_validity_and_records_a_history_entry()
    {
        var license = DomainTestData.License();
        license.Renew(DomainTestData.End.AddYears(1), 20m, Guid.NewGuid(), DomainTestData.End.AddDays(-1));
        Assert.Equal(DomainTestData.End.AddYears(1), license.ExpirationDate);
        Assert.Equal(2, license.StatusHistory.Count);
        Assert.Throws<DomainValidationException>(() => license.Renew(DomainTestData.End, 10, Guid.NewGuid()));
        license.Revoke(DomainTestData.AdminId);
        Assert.Throws<DomainRuleViolationException>(() => license.Renew(DomainTestData.End.AddYears(2), 20, Guid.NewGuid()));
    }

    [Fact]
    public void LicenseActivation_tracks_validation_and_deactivation_once()
    {
        var activation = new LicenseActivation(Guid.NewGuid(), DomainTestData.Machine(), DomainTestData.Instance(), "127.0.0.1", DomainTestData.Start);
        activation.MarkValidated(DomainTestData.Start.AddMinutes(1));
        Assert.Equal(DomainTestData.Start.AddMinutes(1), activation.LastValidatedTime);
        Assert.Throws<DomainValidationException>(() => activation.MarkValidated(DomainTestData.Start.AddMinutes(-1)));
        activation.Deactivate(DomainTestData.Start.AddMinutes(2));
        Assert.Equal(ActivationStatusEnum.Deactivated, activation.Status);
        Assert.Throws<DomainRuleViolationException>(() => activation.MarkValidated());
        Assert.Throws<DomainValidationException>(() => new LicenseActivation(Guid.Empty, DomainTestData.Machine(), DomainTestData.Instance(), "127.0.0.1"));
    }

    [Fact]
    public void LicenseFeature_toggles_access()
    {
        var feature = new Feature("reports", "Reports", "");
        var grant = new LicenseFeature(Guid.NewGuid(), feature);
        Assert.True(grant.IsEnabled);
        grant.Disable();
        Assert.False(grant.IsEnabled);
        grant.Enable();
        Assert.True(grant.IsEnabled);
    }

    [Fact]
    public void LicenseLimit_changes_value_and_rejects_negative_values()
    {
        var limit = new LicenseLimit(Guid.NewGuid(), "users", 10, "users");
        limit.SetValue(25, "seats");
        Assert.Equal(25, limit.Value);
        Assert.Equal("seats", limit.Unit);
        Assert.Throws<DomainValidationException>(() => limit.SetValue(-1));
        Assert.Throws<DomainValidationException>(() => new LicenseLimit(Guid.NewGuid(), "", 1));
    }

    [Fact]
    public void LicenseStatusHistory_captures_valid_status_and_utc_timestamp()
    {
        var changedAt = DateTime.SpecifyKind(DomainTestData.Start, DateTimeKind.Unspecified);
        var history = new LicenseStatusHistory(Guid.NewGuid(), LicenseStatusEnum.SUSPENDED, DomainTestData.AdminId, "review", changedAt);
        Assert.Equal(DateTimeKind.Utc, history.ChangedAt.Kind);
        Assert.Equal("review", history.Reason);
        Assert.Throws<DomainValidationException>(() => new LicenseStatusHistory(Guid.Empty, LicenseStatusEnum.ACTIVE, null));
    }

    [Fact]
    public void SubscriptionRenewal_preserves_old_and_new_expiration_values()
    {
        var renewal = new SubscriptionRenewal(Guid.NewGuid(), 5, DomainTestData.End, DomainTestData.End.AddMonths(1));
        Assert.True(renewal.NewExpirationDate > renewal.PreviousExpirationDate);
        Assert.Throws<DomainValidationException>(() => new SubscriptionRenewal(Guid.NewGuid(), -1, DomainTestData.End, DomainTestData.End.AddDays(1)));
        Assert.Throws<DomainValidationException>(() => new SubscriptionRenewal(Guid.NewGuid(), 1, DomainTestData.End, DomainTestData.End));
    }

    [Fact]
    public void AuditLog_is_immutable_and_requires_action_and_entity_type()
    {
        Assert.Throws<DomainValidationException>(() => new AuditLog("", "License", null, true));
        Assert.Throws<DomainValidationException>(() => new AuditLog("created", "", null, true));
        var audit = new AuditLog("created", "License", "id", true, createdAt: DomainTestData.Start);
        Assert.Equal(DateTimeKind.Utc, audit.CreatedAt.Kind);
        Assert.True(audit.IsSuccess);
        Assert.Throws<DomainValidationException>(() => new AuditLog("created", "License", null, true, userId: Guid.Empty));
    }
}
