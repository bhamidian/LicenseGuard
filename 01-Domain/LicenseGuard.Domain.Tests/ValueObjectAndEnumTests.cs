using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Exceptions;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.ValueObjects;
using Xunit;

namespace LicenseGuard.Domain.Tests;

public sealed class ValueObjectAndEnumTests
{
    [Fact]
    public void MachineId_normalizes_and_compares_by_value()
    {
        var left = MachineId.Create(" machine-a ");
        Assert.Equal("machine-a", left.Value);
        Assert.Equal(left, MachineId.Create("machine-a"));
        Assert.NotEqual(left, MachineId.Create("machine-b"));
        Assert.Throws<DomainValidationException>(() => MachineId.Create("  "));
        Assert.Throws<DomainValidationException>(() => MachineId.Create("bad\nvalue"));
        Assert.Throws<DomainValidationException>(() => MachineId.Create(new string('x', 257)));
        Assert.Throws<DomainValidationException>(() => MachineId.Create(null!));
    }

    [Fact]
    public void InstanceId_normalizes_and_compares_by_value()
    {
        var left = InstanceId.Create(" instance-a ");
        Assert.Equal("instance-a", left.ToString());
        Assert.Equal(left, InstanceId.Create("instance-a"));
        Assert.Throws<DomainValidationException>(() => InstanceId.Create(""));
        Assert.Throws<DomainValidationException>(() => InstanceId.Create(new string('x', 257)));
    }

    [Fact]
    public void LicenseKey_generates_random_256_bit_hex_and_validates_format()
    {
        var first = LicenseKey.Generate();
        var second = LicenseKey.Generate();
        Assert.Equal(64, first.Value.Length);
        Assert.NotEqual(first, second);
        Assert.Equal(first, LicenseKey.Create(first.Value.ToLowerInvariant()));
        Assert.Throws<DomainValidationException>(() => LicenseKey.Create("short"));
        Assert.Throws<DomainValidationException>(() => LicenseKey.Create(new string('z', 64)));
    }

    [Fact]
    public void LicenseSignature_trims_and_validates_size()
    {
        Assert.Equal("signed", LicenseSignature.Create(" signed ").Value);
        Assert.Throws<DomainValidationException>(() => LicenseSignature.Create(" "));
        Assert.Throws<DomainValidationException>(() => LicenseSignature.Create(new string('x', 2049)));
    }

    [Fact]
    public void AutoRenewal_requires_interval_before_enable_and_validates_enum()
    {
        var setting = new AutoRenewal(false);
        Assert.Throws<DomainRuleViolationException>(() => setting.Enable());
        setting.SetPlan(AutoRenwalPlanEnum.Monthly);
        setting.Enable();
        Assert.True(setting.IsEnabled);
        setting.Disable();
        Assert.False(setting.IsEnabled);
        Assert.Throws<DomainValidationException>(() => setting.SetPlan((AutoRenwalPlanEnum)99));
        var configured = new AutoRenewal(false);
        configured.Configure(true, AutoRenwalPlanEnum.Annually);
        Assert.True(configured.IsEnabled);
    }

    [Fact]
    public void ValueObject_equality_is_type_safe_and_hash_consistent()
    {
        var a = MachineId.Create("same");
        var b = MachineId.Create("same");
        var c = InstanceId.Create("same");
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual<ValueObject>(a, c);
    }

    [Fact]
    public void Enums_expose_documented_states_and_intervals()
    {
        Assert.Equal(2, Enum.GetValues<ActivationStatusEnum>().Length);
        Assert.Contains(ActivationStatusEnum.Active, Enum.GetValues<ActivationStatusEnum>());
        Assert.Contains(ActivationStatusEnum.Deactivated, Enum.GetValues<ActivationStatusEnum>());
        Assert.Contains(LicenseStatusEnum.PENDING, Enum.GetValues<LicenseStatusEnum>());
        Assert.Contains(LicenseStatusEnum.ACTIVE, Enum.GetValues<LicenseStatusEnum>());
        Assert.Contains(SubscriptionStatusEnum.GracePeriod, Enum.GetValues<SubscriptionStatusEnum>());
        Assert.Contains(AutoRenwalPlanEnum.Annually, Enum.GetValues<AutoRenwalPlanEnum>());
    }

    [Fact]
    public void Signing_record_has_value_equality()
    {
        var data = new LicenseSigningData("key", Guid.NewGuid(), Guid.NewGuid(), DomainTestData.Start, DomainTestData.End);
        Assert.Equal(data, data with { });
    }
}
