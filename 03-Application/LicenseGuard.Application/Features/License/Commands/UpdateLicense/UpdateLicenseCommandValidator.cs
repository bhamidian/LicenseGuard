using FluentValidation;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Application.Features.License.Commands.UpdateLicense;

public sealed class UpdateLicenseCommandValidator : AbstractValidator<UpdateLicenseCommand>
{
    public UpdateLicenseCommandValidator()
    {
        RuleFor(command => command.LicenseId).NotEmpty();
        RuleFor(command => command.AdminId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Description).MaximumLength(2000);
        RuleFor(command => command.Reason).MaximumLength(500);
        RuleFor(command => command.AutoRenewalPlan)
            .Must(plan => plan is null || Enum.IsDefined(plan.Value));
        RuleFor(command => command.AutoRenewalEnabled)
            .Must((command, enabled) => enabled != true || command.AutoRenewalPlan is not null)
            .WithMessage("An auto-renewal plan is required when enabling auto-renewal.");
        RuleFor(command => command.MaxActivations).GreaterThan(0).When(command => command.MaxActivations.HasValue);
        RuleFor(command => command.FeatureIds)
            .Must(ids => ids is null || (ids.All(id => id != Guid.Empty) && ids.Distinct().Count() == ids.Count))
            .WithMessage("Feature IDs must be non-empty and unique.");
        RuleFor(command => command.Limits)
            .Must(limits => limits is null || limits.Select(limit => limit.Code.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() == limits.Count)
            .WithMessage("Limit codes must be unique.");
        RuleForEach(command => command.Limits).ChildRules(limit =>
        {
            limit.RuleFor(item => item.Code).NotEmpty().MaximumLength(100)
                .Must(code => !string.Equals(code.Trim(), "max_activations", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Update max_activations using the dedicated field.");
            limit.RuleFor(item => item.Value).GreaterThanOrEqualTo(0);
            limit.RuleFor(item => item.Unit).MaximumLength(32);
        });
        RuleFor(command => command)
            .Must(command => command.Description is not null || command.AutoRenewalEnabled is not null ||
                             command.AutoRenewalPlan is not null || command.MaxActivations is not null ||
                             command.FeatureIds is not null || command.Limits is not null)
            .WithMessage("At least one license setting must be provided.");
    }
}
