using LicenseGuard.Domain.Enums;

namespace LicenseGuard.Domain.Entities
{
    public sealed class AutoRenewal
    {
        public AutoRenewal(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
        public bool IsEnabled { get; private set; }
        public void Enable() => IsEnabled = true;
        public void Disable() => IsEnabled = false;
        public AutoRenwalPlanEnum? Plan { get; private set; }
        public void SetPlan(AutoRenwalPlanEnum plan)
        {
            Plan = plan;
        }
        
        
    }
}