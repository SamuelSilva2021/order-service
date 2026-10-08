namespace OrderService.Application.Products;

public sealed record CreateProductCommand(string Name, decimal UnitPrice, string Currency, int AvailableQuantity);
