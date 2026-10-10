using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Common.Models;
using OrderService.Application.Common.Security;
using OrderService.Application.Products.Commands.CreateProduct;
using OrderService.Application.Products.Common;
using OrderService.Application.Products.Queries.ListProducts;

namespace OrderService.Api.Controllers;

[ApiController]
[Authorize]
[Route("products")]
public sealed class ProductsController : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductCommand command,
        [FromServices] CreateProductCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var product = await handler.HandleAsync(command, cancellationToken);

        return Created($"/products/{product.Id}", product);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> List(
        [FromQuery] ListProductsQuery query,
        [FromServices] ListProductsQueryHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(query, cancellationToken));
}
