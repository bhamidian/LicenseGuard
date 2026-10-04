using LicenseGuard.Domain.Dtos;
using MediatR;

namespace LicenseGuard.Application.Features.ProductCatalog.Queries.GetProducts;

public sealed record GetProductsQuery : IRequest<ResultDto<IReadOnlyCollection<ProductCatalogItemResponse>>>;

public sealed record ProductCatalogItemResponse(Guid ProductId, string Code, string Name, string Description);
