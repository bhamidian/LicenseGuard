using LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductFeatures;
using LicenseGuard.Application.Features.ProductCatalog.Queries.GetProductPlans;
using LicenseGuard.Application.Features.ProductCatalog.Queries.GetProducts;
using LicenseGuard.Domain.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LicenseGuard.Endpoint.WebApi.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Roles = "Admin,Customer")]
public sealed class ProductCatalogController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{productId:guid}/plans")]
    public async Task<IActionResult> GetPlans(Guid productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductPlansQuery(productId), cancellationToken);
        return result.IsSuccess ? Ok(result) : CatalogFailure(result);
    }

    [HttpGet("{productId:guid}/features")]
    public async Task<IActionResult> GetFeatures(Guid productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductFeaturesQuery(productId), cancellationToken);
        return result.IsSuccess ? Ok(result) : CatalogFailure(result);
    }

    private IActionResult CatalogFailure<T>(ResultDto<T> result) =>
        result.FailureKind == ResultFailureKind.NotFound ? NotFound(result) : BadRequest(result);
}
