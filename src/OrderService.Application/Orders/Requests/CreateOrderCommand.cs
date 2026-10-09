namespace OrderService.Application.Orders.Requests;

public sealed record CreateOrderCommand(string CustomerId, string Currency, IReadOnlyList<CreateOrderItemCommand> Items);
