namespace OrderService.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Name, decimal UnitPrice, string Currency, int AvailableQuantity);
