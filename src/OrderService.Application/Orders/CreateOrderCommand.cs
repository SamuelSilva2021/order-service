namespace OrderService.Application.Orders;

public sealed record CreateOrderCommand(string CustomerId, string Currency, IReadOnlyList<CreateOrderItemCommand> Items);
