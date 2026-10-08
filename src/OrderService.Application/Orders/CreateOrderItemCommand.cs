namespace OrderService.Application.Orders;

public sealed record CreateOrderItemCommand(Guid ProductId, int Quantity);
