using LicenseGuard.Application.Contracts;
using LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand;
using LicenseGuard.Application.Features.License.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using LicenseGuard.Domain.Records;
using LicenseGuard.Application.Features.Subscription.Commands;
using LicenseGuard.Application.Features.License.Commands.ActivateLicense;
using LicenseGuard.Application.Features.License.Commands.ValidateLicense;
using LicenseGuard.Application.Features.Auth.Commands.RequestLoginCode;
using LicenseGuard.Application.Features.Auth.Commands.VerifyLoginCode;

namespace LicenseGuard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLicenseGuardApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(CreateLicenseCommandHandler).Assembly));
        services.AddScoped<IValidator<CreateLicenseCommand>, CreateLicenseCommandValidator>();
        services.AddScoped<IValidator<ActivateLicenseCommand>, ActivateLicenseCommandValidator>();
        services.AddScoped<IValidator<ValidateLicenseCommand>, ValidateLicenseCommandValidator>();
        services.AddScoped<IValidator<RequestLoginCodeCommand>, RequestLoginCodeCommandValidator>();
        services.AddScoped<IValidator<VerifyLoginCodeCommand>, VerifyLoginCodeCommandValidator>();
        services.AddScoped<IValidator<RenewSubscriptionCommand>, RenewSubscriptionCommandValidator>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IValidator<CreateAuditLogRecord>, AuditLogRecordValidator>();

        return services;
    }
}
