namespace OrderService.Application.Orders.Requests;

public sealed record CreateOrderItemCommand(Guid ProductId, int Quantity);
