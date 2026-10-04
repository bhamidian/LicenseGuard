using LicenseGuard.Application.Contracts;
using LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand;
using LicenseGuard.Application.Features.License.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using LicenseGuard.Domain.Records;
using LicenseGuard.Application.Features.Subscription.Commands;
using LicenseGuard.Application.Features.License.Commands.ActivateLicense;
using LicenseGuard.Application.Features.License.Commands.ChangeLicenseStatus;
using LicenseGuard.Application.Features.License.Commands.ValidateLicense;
using LicenseGuard.Application.Features.Auth.Commands.RequestLoginCode;
using LicenseGuard.Application.Features.Auth.Commands.VerifyLoginCode;
using LicenseGuard.Application.Features.License.Queries.SearchLicenses;
using LicenseGuard.Application.Features.License.Queries.GetLicenseDetails;
using LicenseGuard.Application.Features.License.Commands.DeactivateLicenseActivation;
using LicenseGuard.Application.Features.License.Commands.ExpireLicenses;
using LicenseGuard.Application.Features.License.Queries.GetCustomerLicenses;
using LicenseGuard.Application.Features.ProductCatalog.Queries.GetProducts;
using LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductPlans;
using LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductFeatures;
using LicenseGuard.Application.Features.Subscription.Queries.GetSubscriptionDetails;
using LicenseGuard.Application.Features.License.Commands.UpdateLicense;

namespace LicenseGuard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLicenseGuardApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(CreateLicenseCommandHandler).Assembly));
        services.AddScoped<IValidator<CreateLicenseCommand>, CreateLicenseCommandValidator>();
        services.AddScoped<IValidator<ActivateLicenseCommand>, ActivateLicenseCommandValidator>();
        services.AddScoped<IValidator<ChangeLicenseStatusCommand>, ChangeLicenseStatusCommandValidator>();
        services.AddScoped<IValidator<ValidateLicenseCommand>, ValidateLicenseCommandValidator>();
        services.AddScoped<IValidator<RequestLoginCodeCommand>, RequestLoginCodeCommandValidator>();
        services.AddScoped<IValidator<VerifyLoginCodeCommand>, VerifyLoginCodeCommandValidator>();
        services.AddScoped<IValidator<SearchLicensesQuery>, SearchLicensesQueryValidator>();
        services.AddScoped<IValidator<GetLicenseDetailsQuery>, GetLicenseDetailsQueryValidator>();
        services.AddScoped<IValidator<DeactivateLicenseActivationCommand>, DeactivateLicenseActivationCommandValidator>();
        services.AddScoped<IValidator<ExpireLicensesCommand>, ExpireLicensesCommandValidator>();
        services.AddScoped<IValidator<GetCustomerLicensesQuery>, GetCustomerLicensesQueryValidator>();
        services.AddScoped<IValidator<GetProductPlansQuery>, GetProductPlansQueryValidator>();
        services.AddScoped<IValidator<GetProductFeaturesQuery>, GetProductFeaturesQueryValidator>();
        services.AddScoped<IValidator<GetSubscriptionDetailsQuery>, GetSubscriptionDetailsQueryValidator>();
        services.AddScoped<IValidator<UpdateLicenseCommand>, UpdateLicenseCommandValidator>();
        services.AddScoped<IValidator<RenewSubscriptionCommand>, RenewSubscriptionCommandValidator>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IValidator<CreateAuditLogRecord>, AuditLogRecordValidator>();

        return services;
    }
}
