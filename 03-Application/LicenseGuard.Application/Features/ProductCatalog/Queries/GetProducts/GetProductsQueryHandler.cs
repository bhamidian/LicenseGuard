using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Repositories;
using MediatR;

namespace LicenseGuard.Application.Features.ProductCatalog.Queries.GetProducts;

public sealed class GetProductsQueryHandler(IProductCatalogRepository catalog)
    : IRequestHandler<GetProductsQuery, ResultDto<IReadOnlyCollection<ProductCatalogItemResponse>>>
{
    public async Task<ResultDto<IReadOnlyCollection<ProductCatalogItemResponse>>> Handle(
        GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await catalog.GetProductsAsync(cancellationToken);
        var response = products.Select(product => new ProductCatalogItemResponse(
            product.ProductId, product.Code, product.Name, product.Description)).ToArray();
        return ResultDto<IReadOnlyCollection<ProductCatalogItemResponse>>.Success("Products retrieved.", response);
    }
}
