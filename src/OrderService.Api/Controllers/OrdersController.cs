using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Common.Models;
using OrderService.Application.Orders.Commands.CancelOrder;
using OrderService.Application.Orders.Commands.ConfirmOrder;
using OrderService.Application.Orders.Commands.CreateOrder;
using OrderService.Application.Orders.Common;
using OrderService.Application.Orders.Queries.GetOrder;
using OrderService.Application.Orders.Queries.ListOrders;

namespace OrderService.Api.Controllers;

[ApiController]
[Authorize]
[Route("orders")]
public sealed class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderCommand command,
        [FromServices] CreateOrderCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var order = await handler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<OrderDto>> Confirm(
        Guid id,
        [FromServices] ConfirmOrderCommandHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new ConfirmOrderCommand(id), cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(
        Guid id,
        [FromServices] CancelOrderCommandHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new CancelOrderCommand(id), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Get(
        Guid id,
        [FromServices] GetOrderQueryHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetOrderQuery(id), cancellationToken));

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> List(
        [FromQuery] ListOrdersQuery query,
        [FromServices] ListOrdersQueryHandler handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(query, cancellationToken));
}
