namespace OrderService.Application.Products.Requests;

public sealed record CreateProductCommand(string Name, decimal UnitPrice, string Currency, int AvailableQuantity);
