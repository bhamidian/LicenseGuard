using FluentValidation;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.License.Queries.SearchLicenses;

public sealed class SearchLicensesQueryHandler(
    IValidator<SearchLicensesQuery> validator,
    ILicenseRepository licenses)
    : IRequestHandler<SearchLicensesQuery, ResultDto<SearchLicensesResponse>>
{
    public async Task<ResultDto<SearchLicensesResponse>> Handle(
        SearchLicensesQuery query, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<SearchLicensesResponse>.Fail("License search request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        var now = DateTime.UtcNow;
        var expiresAfter = query.ExpiresWithinDays is not null ? now : (DateTime?)null;
        var expiresBefore = query.ExpiresWithinDays is { } days
            ? now.AddDays(days)
            : (DateTime?)null;
        var page = await licenses.SearchAsync(new LicenseSearchCriteria(
            string.IsNullOrWhiteSpace(query.LicenseKey) ? null : query.LicenseKey.Trim(),
            query.CustomerId, query.ProductId, query.PlanId, query.Status, expiresAfter, expiresBefore,
            (query.Page - 1) * query.PageSize, query.PageSize), cancellationToken);

        var items = page.Items.Select(item => new LicenseListItemResponse(
            item.LicenseId, MaskKey(item.LicenseKey), item.CustomerId, item.ProductId, item.PlanId,
            item.Status, item.CreatedAt, item.StartDate, item.ExpirationDate, item.MaxActivations,
            item.ActiveActivations, item.EnabledFeatureCount, item.AutoRenewalEnabled)).ToArray();

        return ResultDto<SearchLicensesResponse>.Success("Licenses retrieved.", new SearchLicensesResponse(
            items, query.Page, query.PageSize, page.TotalCount,
            (int)Math.Ceiling(page.TotalCount / (double)query.PageSize)));
    }

    private static string MaskKey(string key) => $"{new string('*', Math.Max(0, key.Length - 8))}{key[^8..]}";
}
