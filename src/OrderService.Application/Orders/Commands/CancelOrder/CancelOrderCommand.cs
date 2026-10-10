namespace OrderService.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId);
