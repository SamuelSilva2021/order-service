using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Auth;
using OrderService.Application.Common;
using OrderService.Application.Products;

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
        [FromServices] CreateProductUseCase useCase,
        CancellationToken cancellationToken)
    {
        var product = await useCase.ExecuteAsync(command, cancellationToken);

        return Created($"/products/{product.Id}", product);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> List(
        [FromQuery] ListProductsQuery query,
        [FromServices] ListProductsUseCase useCase,
        CancellationToken cancellationToken) =>
        Ok(await useCase.ExecuteAsync(query, cancellationToken));
}
