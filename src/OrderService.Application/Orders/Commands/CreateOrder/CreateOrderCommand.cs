namespace OrderService.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(string CustomerId, string Currency, IReadOnlyList<CreateOrderItemCommand> Items);
