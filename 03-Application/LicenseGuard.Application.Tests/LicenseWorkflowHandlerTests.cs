using System.Data;
using FluentValidation;
using LicenseGuard.Application.Contracts;
using LicenseGuard.Application.Features.License.Commands.ActivateLicense;
using LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand;
using LicenseGuard.Application.Features.License.Commands.DeactivateLicenseActivation;
using LicenseGuard.Application.Features.License.Commands.ExpireLicenses;
using LicenseGuard.Application.Features.License.Commands.UpdateLicense;
using LicenseGuard.Application.Features.License.Services;
using LicenseGuard.Application.Features.Subscription.Commands;
using LicenseGuard.Application.Features.Subscription.Events;
using LicenseGuard.Application.Features.Subscription.Records;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Domain.ValueObjects;
using LicenseGuard.Infrstructure.SecurityService.Contracts;
using MediatR;
using Xunit;

namespace LicenseGuard.Application.Tests;

public sealed class LicenseWorkflowHandlerTests
{
    private static readonly Guid AdminId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Audit_service_builds_activation_record_without_license_secret_or_user_identity()
    {
        var licenseId = Guid.NewGuid();
        var activationId = Guid.NewGuid();
        var service = new AuditLogService(new AuditLogRecordValidator());

        var result = await service.ValidateLicenseActivatedAsync(licenseId, activationId, "127.0.0.10", default);

        Assert.True(result.IsSuccess);
        Assert.Equal("license.activation.created", result.Data!.Action);
        Assert.Equal("LicenseActivation", result.Data.EntityType);
        Assert.Equal(activationId.ToString(), result.Data.EntityId);
        Assert.Equal(licenseId, result.Data.LicenseId);
        Assert.Equal("127.0.0.10", result.Data.IpAddress);
        Assert.Null(result.Data.UserId);
        Assert.Null(result.Data.Metadata);
    }

    [Fact]
    public async Task Audit_service_captures_renewal_actor_amount_and_expiration_change()
    {
        var service = new AuditLogService(new AuditLogRecordValidator());
        var subscriptionId = Guid.NewGuid();
        var licenseId = Guid.NewGuid();
        var oldEnd = DateTime.UtcNow;
        var newEnd = oldEnd.AddDays(30);

        var result = await service.ValidateSubscriptionRenewedAsync(subscriptionId, UserId, licenseId, 17.5m,
            oldEnd, newEnd, default);

        Assert.True(result.IsSuccess);
        Assert.Equal("subscription.renewed", result.Data!.Action);
        Assert.Equal(subscriptionId.ToString(), result.Data.EntityId);
        Assert.Equal(UserId, result.Data.UserId);
        Assert.Equal(licenseId, result.Data.LicenseId);
        Assert.Equal(oldEnd.ToString("O"), result.Data.OldValues);
        Assert.Equal(newEnd.ToString("O"), result.Data.NewValues);
        Assert.Equal("Amount=17.5", result.Data.Metadata);
    }

    [Fact]
    public async Task Issuance_signs_license_and_commits_audit_atomically()
    {
        var data = TestData.CreateCatalog();
        var repository = new LicenseRepositoryStub { IssuanceSubscription = data.Subscription };
        var unitOfWork = new UnitOfWorkStub();
        var audits = new AuditRepositoryStub();
        var signer = new SignerStub();
        var handler = new CreateLicenseCommandHandler(new CreateLicenseCommandValidator(),
            new LicenseServiceStub(), repository, repository, audits, unitOfWork, signer, new AuditServiceStub());

        var result = await handler.Handle(new CreateLicenseCommand(data.Subscription.Id, AdminId, UserId, 2), default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.License);
        Assert.NotNull(repository.License.Signature);
        Assert.Equal(1, signer.SignCount);
        Assert.Single(audits.Records);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(1, unitOfWork.Transaction.CommitCount);
        Assert.Equal(IsolationLevel.Serializable, unitOfWork.IsolationLevel);
    }

    [Fact]
    public async Task Issuance_audit_validation_failure_does_not_save_or_commit()
    {
        var data = TestData.CreateCatalog();
        var repository = new LicenseRepositoryStub { IssuanceSubscription = data.Subscription };
        var unitOfWork = new UnitOfWorkStub();
        var handler = new CreateLicenseCommandHandler(new CreateLicenseCommandValidator(),
            new LicenseServiceStub(), repository, repository, new AuditRepositoryStub(), unitOfWork,
            new SignerStub(), new AuditServiceStub { ReturnFailure = true });

        var result = await handler.Handle(new CreateLicenseCommand(data.Subscription.Id, AdminId, UserId, 1), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(0, unitOfWork.SaveCount);
        Assert.Equal(0, unitOfWork.Transaction.CommitCount);
    }

    [Fact]
    public async Task Activation_rejects_invalid_signature_without_creating_activation()
    {
        var data = TestData.CreateLicense(maxActivations: 2);
        var repository = new LicenseRepositoryStub { License = data.License };
        var unitOfWork = new UnitOfWorkStub();
        var auditRepository = new AuditRepositoryStub();
        var handler = new ActivateLicenseCommandHandler(new ActivateLicenseCommandValidator(), repository,
            repository, unitOfWork, new SignerStub { IsValid = false }, auditRepository, new AuditServiceStub());

        var result = await handler.Handle(TestData.ActivateCommand(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureKind.Conflict, result.FailureKind);
        Assert.Empty(data.License.Activations);
        Assert.Empty(auditRepository.Records);
        Assert.Equal(0, unitOfWork.SaveCount);
        Assert.Equal(0, unitOfWork.Transaction.CommitCount);
    }

    [Fact]
    public async Task Activation_rejects_when_active_activation_quota_is_full()
    {
        var data = TestData.CreateLicense(maxActivations: 1);
        data.License.SetSignature(LicenseSignature.Create("valid-signature"));
        data.License.Activate(MachineId.Create("machine-existing"), InstanceId.Create("instance-existing"), "127.0.0.1");
        var repository = new LicenseRepositoryStub { License = data.License };
        var unitOfWork = new UnitOfWorkStub();
        var auditRepository = new AuditRepositoryStub();
        var handler = new ActivateLicenseCommandHandler(new ActivateLicenseCommandValidator(), repository,
            repository, unitOfWork, new SignerStub(), auditRepository, new AuditServiceStub());

        var result = await handler.Handle(TestData.ActivateCommand(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultFailureKind.Conflict, result.FailureKind);
        Assert.Single(data.License.Activations);
        Assert.Empty(auditRepository.Records);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Successful_activation_is_audited_with_activation_id_and_client_ip()
    {
        var data = TestData.CreateLicense(maxActivations: 2);
        data.License.SetSignature(LicenseSignature.Create("valid-signature"));
        var repository = new LicenseRepositoryStub { License = data.License };
        var auditRepository = new AuditRepositoryStub();
        var unitOfWork = new UnitOfWorkStub();
        var handler = new ActivateLicenseCommandHandler(new ActivateLicenseCommandValidator(), repository,
            repository, unitOfWork, new SignerStub(), auditRepository, new AuditServiceStub());

        var result = await handler.Handle(TestData.ActivateCommand(), default);

        Assert.True(result.IsSuccess);
        var audit = Assert.Single(auditRepository.Records);
        Assert.Equal("license.activation.created", audit.Action);
        Assert.Equal(result.Data!.ActivationId.ToString(), audit.EntityId);
        Assert.Equal("127.0.0.2", audit.IpAddress);
        Assert.Null(audit.UserId);
        Assert.Equal(data.License.Id, audit.LicenseId);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(1, unitOfWork.Transaction.CommitCount);
    }

    [Fact]
    public async Task Renewal_updates_license_expiration_history_and_signature()
    {
        var data = TestData.CreateLicense(maxActivations: 2);
        data.License.SetSignature(LicenseSignature.Create("old-signature"));
        var repository = new LicenseRepositoryStub { RenewalSubscription = data.Subscription };
        var renewalRepository = new RenewalRepositoryStub();
        var unitOfWork = new UnitOfWorkStub();
        var signer = new SignerStub();
        var auditRepository = new AuditRepositoryStub();
        var licenseRenewalHandler = new RenewLicenseAfterSubscriptionRenewalHandler(repository, signer);
        var publisher = new InlinePublisher(licenseRenewalHandler);
        var newEnd = data.Subscription.EndDate.AddDays(30);
        var handler = new RenewSubscriptionCommandHandler(new RenewSubscriptionCommandValidator(), repository,
            renewalRepository, unitOfWork, publisher, auditRepository, new AuditServiceStub());

        var result = await handler.Handle(new RenewSubscriptionCommand(data.Subscription.Id, UserId, 25m, newEnd), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(newEnd, data.Subscription.EndDate);
        Assert.Equal(newEnd, data.License.ExpirationDate);
        Assert.Equal(2, data.License.StatusHistory.Count);
        Assert.Equal("signature-1", data.License.Signature.Value);
        Assert.Single(data.Subscription.Renewals);
        var renewalAudit = Assert.Single(auditRepository.Records);
        Assert.Equal("subscription.renewed", renewalAudit.Action);
        Assert.Equal(data.Subscription.Id.ToString(), renewalAudit.EntityId);
        Assert.Equal(data.License.Id, renewalAudit.LicenseId);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(1, unitOfWork.Transaction.CommitCount);
    }

    [Fact]
    public async Task Renewal_without_current_license_still_creates_subscription_audit()
    {
        var (subscription, _) = TestData.CreateCatalog();
        var repository = new LicenseRepositoryStub { RenewalSubscription = subscription };
        var auditRepository = new AuditRepositoryStub();
        var unitOfWork = new UnitOfWorkStub();
        var handler = new RenewSubscriptionCommandHandler(new RenewSubscriptionCommandValidator(), repository,
            new RenewalRepositoryStub(), unitOfWork, new InlinePublisher(new RenewLicenseAfterSubscriptionRenewalHandler(repository, new SignerStub())),
            auditRepository, new AuditServiceStub());
        var newEnd = subscription.EndDate.AddDays(15);

        var result = await handler.Handle(new RenewSubscriptionCommand(subscription.Id, UserId, 10m, newEnd), default);

        Assert.True(result.IsSuccess);
        var audit = Assert.Single(auditRepository.Records);
        Assert.Equal("subscription.renewed", audit.Action);
        Assert.Equal(subscription.Id.ToString(), audit.EntityId);
        Assert.Equal(UserId, audit.UserId);
        Assert.Null(audit.LicenseId);
        Assert.Equal("Amount=10", audit.Metadata);
        Assert.Equal(1, unitOfWork.Transaction.CommitCount);
    }

    [Fact]
    public async Task Update_resigns_changed_entitlements_and_records_audit()
    {
        var data = TestData.CreateLicense(maxActivations: 2);
        data.License.SetSignature(LicenseSignature.Create("before"));
        var repository = new LicenseRepositoryStub { License = data.License };
        var audits = new AuditRepositoryStub();
        var unitOfWork = new UnitOfWorkStub();
        var signer = new SignerStub();
        var handler = new UpdateLicenseCommandHandler(new UpdateLicenseCommandValidator(), repository,
            audits, new AuditServiceStub(), unitOfWork, signer);

        var result = await handler.Handle(new UpdateLicenseCommand(data.License.Id, AdminId, UserId,
            "updated", null, null, 3, null, null, "requested update"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("signature-1", data.License.Signature.Value);
        Assert.Equal(3, result.Data!.MaxActivations);
        Assert.Single(audits.Records);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(1, unitOfWork.Transaction.CommitCount);
    }

    [Fact]
    public async Task Deactivation_releases_activation_and_audits_reason()
    {
        var data = TestData.CreateLicense(maxActivations: 2);
        var activation = data.License.Activate(MachineId.Create("machine-a"), InstanceId.Create("instance-a"), "127.0.0.1");
        var repository = new LicenseRepositoryStub { License = data.License };
        var audits = new AuditRepositoryStub();
        var unitOfWork = new UnitOfWorkStub();
        var handler = new DeactivateLicenseActivationCommandHandler(new DeactivateLicenseActivationCommandValidator(),
            repository, audits, new AuditServiceStub(), unitOfWork);

        var result = await handler.Handle(new DeactivateLicenseActivationCommand(data.License.Id, activation.Id,
            AdminId, UserId, "customer request"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(ActivationStatusEnum.Deactivated, activation.Status);
        Assert.Equal("customer request", Assert.Single(audits.Records).Metadata);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(1, unitOfWork.Transaction.CommitCount);
    }

    [Fact]
    public async Task Expiration_changes_status_history_and_audit_for_candidate()
    {
        var data = TestData.CreateExpiredLicense();
        var repository = new LicenseRepositoryStub { License = data.License, ExpiredIds = [data.License.Id] };
        var audits = new AuditRepositoryStub();
        var unitOfWork = new UnitOfWorkStub();
        var handler = new ExpireLicensesCommandHandler(new ExpireLicensesCommandValidator(), repository,
            audits, new AuditServiceStub(), unitOfWork);

        var result = await handler.Handle(new ExpireLicensesCommand(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data);
        Assert.Equal(LicenseStatusEnum.EXPIRED, data.License.LicenseStatus);
        Assert.Equal(2, data.License.StatusHistory.Count);
        Assert.Single(audits.Records);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(1, unitOfWork.Transaction.CommitCount);
    }

    private static class TestData
    {
        public static (Subscription Subscription, Feature Feature) CreateCatalog(DateTime? start = null, DateTime? end = null)
        {
            var customer = new Customer(Guid.NewGuid());
            var product = new Product("suite", "Suite", "Suite product");
            var plan = product.AddPlan("Pro", "Professional", 20m);
            var feature = new Feature("reports", "Reports", "Reporting");
            product.AddFeature(feature);
            plan.AddFeature(feature);
            var subscription = new Subscription(customer, product, plan,
                start ?? DateTime.UtcNow.AddDays(-1), end ?? DateTime.UtcNow.AddDays(30), SubscriptionStatusEnum.Active);
            return (subscription, feature);
        }

        public static (Subscription Subscription, Feature Feature, License License) CreateLicense(int maxActivations)
        {
            var (subscription, feature) = CreateCatalog();
            var license = new License(subscription.Id, AdminId, LicenseKey.Create(new string('A', 64)),
                LicenseStatusEnum.ACTIVE, subscription.StartDate, subscription.EndDate, maxActivations,
                subscription.CustomerId, subscription.ProductId, subscription.PlanId);
            license.AddSnapshotFeature(feature);
            subscription.AddLicense(license);
            return (subscription, feature, license);
        }

        public static (Subscription Subscription, Feature Feature, License License) CreateExpiredLicense()
        {
            var (subscription, feature) = CreateCatalog(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1));
            var license = new License(subscription.Id, AdminId, LicenseKey.Create(new string('B', 64)),
                LicenseStatusEnum.ACTIVE, subscription.StartDate, subscription.EndDate, 1,
                subscription.CustomerId, subscription.ProductId, subscription.PlanId);
            license.AddSnapshotFeature(feature);
            subscription.AddLicense(license);
            return (subscription, feature, license);
        }

        public static ActivateLicenseCommand ActivateCommand() =>
            new(new string('A', 64), "machine-new", "instance-new", "127.0.0.2");
    }

    private sealed class LicenseRepositoryStub : ILicenseRepository, ISubscriptionRepository, ILicenseActivationRepository
    {
        public License? License { get; set; }
        public Subscription? IssuanceSubscription { get; set; }
        public Subscription? RenewalSubscription { get; set; }
        public IReadOnlyCollection<Guid> ExpiredIds { get; set; } = Array.Empty<Guid>();
        public List<LicenseStatusHistory> AddedHistory { get; } = [];

        public License Create(CreateLicenseRecord record, Subscription subscription)
        {
            var license = new License(record.SubscriptionId, record.IssuedByAdminId, record.Key, record.Status,
                record.StartDate, record.ExpirationDate, record.MaxActivations, record.CustomerId,
                record.ProductId, record.PlanId, record.PolicyVersion);
            foreach (var feature in subscription.Plan.PlanFeatures.Select(link => link.Feature)
                         .Where(feature => record.FeatureIds is null || record.FeatureIds.Contains(feature.Id)))
                license.AddSnapshotFeature(feature);
            subscription.AddLicense(license);
            License = license;
            return license;
        }

        public Task<License?> GetForStatusChangeAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(License);
        public Task<License?> GetForActivationDeactivationAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(License);
        public Task<License?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(License);
        public Task<IReadOnlyCollection<Feature>> GetAvailableFeaturesForPlanAsync(Guid planId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Feature>>(License?.Subscription.Plan.PlanFeatures.Select(link => link.Feature).ToArray() ?? []);
        public Task<IReadOnlyCollection<Guid>> GetExpiredLicenseIdsAsync(DateTime asOf, int batchSize, CancellationToken cancellationToken = default) => Task.FromResult(ExpiredIds);
        public void AddStatusHistory(LicenseStatusHistory history) => AddedHistory.Add(history);
        public Task<LicenseSearchPageRecord> SearchAsync(LicenseSearchCriteria criteria, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<License?> GetDetailsAsync(Guid licenseId, CancellationToken cancellationToken = default) => Task.FromResult(License);
        public Task<License?> GetForActivationAsync(LicenseKeyLookupRecord record, CancellationToken cancellationToken = default) => Task.FromResult(License);
        public Task<License?> GetForValidationAsync(LicenseKeyLookupRecord record, CancellationToken cancellationToken = default) => Task.FromResult(License);
        public LicenseActivation Create(CreateLicenseActivationRecord record) =>
            new(record.LicenseId, record.MachineId, record.InstanceId, record.IpAddress, record.ActivatedAt);
        public Task<Subscription?> GetForLicenseIssuanceAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(IssuanceSubscription);
        public Task<Subscription?> GetForRenewalAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(RenewalSubscription);
        public Task<Subscription?> GetDetailsAsync(Guid id, Guid? customerId = null, CancellationToken cancellationToken = default) => Task.FromResult(RenewalSubscription ?? IssuanceSubscription);
    }

    private sealed class AuditRepositoryStub : IAuditLogRepository
    {
        public List<CreateAuditLogRecord> Records { get; } = [];
        public AuditLog Create(CreateAuditLogRecord record)
        {
            Records.Add(record);
            return new AuditLog(record.Action, record.EntityType, record.EntityId, record.IsSuccess,
                record.UserId, record.LicenseId, record.CorrelationId, record.IpAddress, record.UserAgent,
                record.OldValues, record.NewValues, record.Metadata, record.CreatedAt);
        }
        public AuditLog Create(CreateAuditLogRecord record, License license)
        {
            var audit = Create(record);
            license.AddAuditLog(audit);
            return audit;
        }
    }

    private sealed class AuditServiceStub : IAuditLogService
    {
        public bool ReturnFailure { get; init; }
        public Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseIssuedAsync(Guid adminId, Guid userId, Guid licenseId, CancellationToken cancellationToken) => Result("license.issued", userId, licenseId);
        public Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseActivatedAsync(Guid licenseId, Guid activationId, string ipAddress, CancellationToken cancellationToken) =>
            Task.FromResult(ReturnFailure
                ? ResultDto<CreateAuditLogRecord>.Fail("audit invalid")
                : ResultDto<CreateAuditLogRecord>.Success("ok", new CreateAuditLogRecord(
                    "license.activation.created", "LicenseActivation", activationId.ToString(), true,
                    LicenseId: licenseId, IpAddress: ipAddress, NewValues: "Active")));
        public Task<ResultDto<CreateAuditLogRecord>> ValidateSubscriptionRenewedAsync(Guid subscriptionId, Guid userId, Guid? licenseId, decimal amount, DateTime oldExpirationDate, DateTime newExpirationDate, CancellationToken cancellationToken) =>
            Task.FromResult(ReturnFailure
                ? ResultDto<CreateAuditLogRecord>.Fail("audit invalid")
                : ResultDto<CreateAuditLogRecord>.Success("ok", new CreateAuditLogRecord(
                    "subscription.renewed", "Subscription", subscriptionId.ToString(), true,
                    UserId: userId, LicenseId: licenseId, OldValues: oldExpirationDate.ToString("O"),
                    NewValues: newExpirationDate.ToString("O"), Metadata: $"Amount={amount}")));
        public Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseStatusChangedAsync(Guid adminId, Guid userId, Guid licenseId, string action, string oldStatus, string newStatus, string? reason, CancellationToken cancellationToken) => Result(action, userId, licenseId, reason, oldStatus, newStatus);
        public Task<ResultDto<CreateAuditLogRecord>> ValidateActivationDeactivatedAsync(Guid adminId, Guid userId, Guid licenseId, Guid activationId, string? reason, CancellationToken cancellationToken) => Result("activation.deactivated", userId, licenseId, reason);
        public Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseExpiredAsync(Guid licenseId, string oldStatus, DateTime processedAt, CancellationToken cancellationToken) => Result("license.expired", null, licenseId, oldValues: oldStatus);
        public Task<ResultDto<CreateAuditLogRecord>> ValidateLicenseUpdatedAsync(Guid adminId, Guid userId, Guid licenseId, string oldValues, string newValues, string? reason, CancellationToken cancellationToken) => Result("license.updated", userId, licenseId, reason, oldValues, newValues);

        private Task<ResultDto<CreateAuditLogRecord>> Result(string action, Guid? userId, Guid licenseId,
            string? metadata = null, string? oldValues = null, string? newValues = null)
        {
            if (ReturnFailure)
                return Task.FromResult(ResultDto<CreateAuditLogRecord>.Fail("audit invalid"));
            return Task.FromResult(ResultDto<CreateAuditLogRecord>.Success("ok",
                new CreateAuditLogRecord(action, "License", licenseId.ToString(), true, userId, licenseId,
                    Metadata: metadata, OldValues: oldValues, NewValues: newValues)));
        }
    }

    private sealed class UnitOfWorkStub : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public IsolationLevel IsolationLevel { get; private set; }
        public TransactionStub Transaction { get; } = new();
        public Task<IUnitOfWorkTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            IsolationLevel = isolationLevel;
            return Task.FromResult<IUnitOfWorkTransaction>(Transaction);
        }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) { SaveCount++; return Task.FromResult(1); }
    }

    private sealed class TransactionStub : IUnitOfWorkTransaction
    {
        public int CommitCount { get; private set; }
        public Task CommitAsync(CancellationToken cancellationToken = default) { CommitCount++; return Task.CompletedTask; }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class SignerStub : ILicenseSigner
    {
        public bool IsValid { get; init; } = true;
        public int SignCount { get; private set; }
        public LicenseSignature Sign(LicenseSigningData data) => LicenseSignature.Create($"signature-{++SignCount}");
        public bool Verify(LicenseSigningData data, LicenseSignature signature) => IsValid;
    }

    private sealed class LicenseServiceStub : ILicenseService
    {
        public LicenseKey GenerateKey() => LicenseKey.Create(new string('C', 64));
    }

    private sealed class RenewalRepositoryStub : ISubscriptionRenewalRepository
    {
        public SubscriptionRenewal Create(CreateSubscriptionRenewalRecord record) =>
            new(record.SubscriptionId, record.Amount, record.PreviousExpirationDate, record.NewExpirationDate, record.RenewedAt);
    }

    private sealed class InlinePublisher(RenewLicenseAfterSubscriptionRenewalHandler handler) : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) =>
            notification is SubscriptionRenewedNotification renewal
                ? handler.Handle(renewal, cancellationToken)
                : Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Publish((object)notification, cancellationToken);
    }
}
