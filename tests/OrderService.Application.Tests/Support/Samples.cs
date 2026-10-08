using OrderService.Domain.Common;
using OrderService.Domain.Orders;
using OrderService.Domain.Products;

namespace OrderService.Application.Tests.Support;

public static class Samples
{
    public static readonly Currency Brl = Currency.Of("BRL");

    public static Product Product(int availableQuantity = 10, decimal unitPrice = 100m) =>
        Domain.Products.Product.Create("Keyboard", unitPrice, Brl, availableQuantity);

    public static Order PlacedOrder(params (Product Product, int Quantity)[] lines) =>
        Order.Place(
            "customer-1",
            Brl,
            lines.Select(line => new OrderItem(line.Product.Id, line.Product.UnitPrice, line.Quantity)),
            DateTimeOffset.UtcNow);

    public static Order ConfirmedOrder(params (Product Product, int Quantity)[] lines)
    {
        var order = PlacedOrder(lines);
        order.Confirm();
        return order;
    }
}
