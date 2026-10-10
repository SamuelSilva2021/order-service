namespace OrderService.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderItemCommand(Guid ProductId, int Quantity);
