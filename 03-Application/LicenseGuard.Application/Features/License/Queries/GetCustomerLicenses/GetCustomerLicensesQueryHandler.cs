using FluentValidation;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.License.Queries.GetCustomerLicenses;

public sealed class GetCustomerLicensesQueryHandler(
    IValidator<GetCustomerLicensesQuery> validator,
    ICustomerRepository customers,
    ILicenseRepository licenses)
    : IRequestHandler<GetCustomerLicensesQuery, ResultDto<CustomerLicensesResponse>>
{
    public async Task<ResultDto<CustomerLicensesResponse>> Handle(
        GetCustomerLicensesQuery query, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<CustomerLicensesResponse>.Fail("Customer license request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        var customer = await customers.GetByAppUserIdAsync(query.AppUserId, cancellationToken);
        if (customer is null)
            return ResultDto<CustomerLicensesResponse>.Fail("Customer profile was not found.",
                failureKind: ResultFailureKind.NotFound);

        var page = await licenses.SearchAsync(new LicenseSearchCriteria(
            null, customer.Id, null, null, null, null, null,
            (query.Page - 1) * query.PageSize, query.PageSize), cancellationToken);

        var items = page.Items.Select(item => new CustomerLicenseItemResponse(
            item.LicenseId, item.LicenseKey, item.ProductId, item.PlanId, item.Status,
            item.CreatedAt, item.StartDate, item.ExpirationDate, item.MaxActivations,
            item.ActiveActivations, item.EnabledFeatureCount, item.AutoRenewalEnabled)).ToArray();

        return ResultDto<CustomerLicensesResponse>.Success("Customer licenses retrieved.",
            new CustomerLicensesResponse(items, query.Page, query.PageSize, page.TotalCount,
                (int)Math.Ceiling(page.TotalCount / (double)query.PageSize)));
    }
}
