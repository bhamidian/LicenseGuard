using LicenseGuard.Domain.Enums;
using LicenseGuard.Domain.Exceptions;

namespace LicenseGuard.Domain.Entities
{
    public sealed class AutoRenewal
    {
        public AutoRenewal(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
        public bool IsEnabled { get; private set; }
        public void Enable()
        {
            if (Plan is null)
                throw new DomainRuleViolationException("Choose a renewal interval before enabling auto-renewal.");
            IsEnabled = true;
        }
        public void Disable() => IsEnabled = false;
        public AutoRenwalPlanEnum? Plan { get; private set; }
        public void SetPlan(AutoRenwalPlanEnum plan)
        {
            if (!Enum.IsDefined(plan))
                throw new DomainValidationException("Auto-renewal interval is invalid.");
            Plan = plan;
        }

        public void Configure(bool isEnabled, AutoRenwalPlanEnum plan)
        {
            SetPlan(plan);
            IsEnabled = isEnabled;
        }


    }
}
