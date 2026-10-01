using FluentValidation;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Application.Features.License.Services;

public sealed class AuditLogRecordValidator : AbstractValidator<CreateAuditLogRecord>
{
    public AuditLogRecordValidator()
    {
        RuleFor(x => x.Action).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).MaximumLength(200);
        RuleFor(x => x.UserId).Must(x => x is null || x != Guid.Empty);
        RuleFor(x => x.LicenseId).Must(x => x is null || x != Guid.Empty);
        RuleFor(x => x.CorrelationId).Must(x => x is null || x != Guid.Empty);
        RuleFor(x => x.IpAddress).MaximumLength(64);
        RuleFor(x => x.UserAgent).MaximumLength(512);
    }
}
