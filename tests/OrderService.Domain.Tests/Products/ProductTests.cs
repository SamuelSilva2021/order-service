using OrderService.Domain.Common;
using OrderService.Domain.Products;

namespace OrderService.Domain.Tests.Products;

public class ProductTests
{
    private static readonly Currency Brl = Currency.Of("BRL");

    private static Product CreateProduct(int availableQuantity = 10) =>
        Product.Create("Keyboard", 100m, Brl, availableQuantity);

    [Fact]
    public void Reserve_DecrementsAvailableQuantity()
    {
        var product = CreateProduct(10);

        product.Reserve(4);

        Assert.Equal(6, product.AvailableQuantity);
    }

    [Fact]
    public void Reserve_ExactlyAvailable_Succeeds()
    {
        var product = CreateProduct(3);

        product.Reserve(3);

        Assert.Equal(0, product.AvailableQuantity);
    }

    [Fact]
    public void Reserve_BeyondAvailable_ThrowsAndKeepsStock()
    {
        var product = CreateProduct(3);

        Assert.Throws<InsufficientStockException>(() => product.Reserve(4));
        Assert.Equal(3, product.AvailableQuantity);
    }

    [Fact]
    public void Release_IncrementsAvailableQuantity()
    {
        var product = CreateProduct(3);

        product.Release(2);

        Assert.Equal(5, product.AvailableQuantity);
    }

    [Fact]
    public void ToOrderItem_CapturesCurrentPrice()
    {
        var product = CreateProduct();

        var item = product.ToOrderItem(2, Brl);

        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(100m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void ToOrderItem_BeyondStock_Throws()
    {
        var product = CreateProduct(1);

        Assert.Throws<InsufficientStockException>(() => product.ToOrderItem(2, Brl));
    }

    [Fact]
    public void ToOrderItem_DifferentCurrency_Throws()
    {
        var product = CreateProduct();

        Assert.Throws<DomainException>(() => product.ToOrderItem(1, Currency.Of("USD")));
    }

    [Fact]
    public void ToOrderItem_DoesNotChangeStock()
    {
        var product = CreateProduct(5);

        product.ToOrderItem(2, Brl);

        Assert.Equal(5, product.AvailableQuantity);
    }

    [Theory]
    [InlineData("", 10, 1)]
    [InlineData("Keyboard", 0, 1)]
    [InlineData("Keyboard", 10, -1)]
    public void Create_InvalidData_Throws(string name, double price, int quantity)
    {
        Assert.Throws<DomainException>(() => Product.Create(name, (decimal)price, Brl, quantity));
    }
}
