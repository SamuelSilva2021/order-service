using OrderService.Domain.Common;
using OrderService.Domain.Orders;

namespace OrderService.Domain.Tests.Orders;

public class OrderItemTests
{
    [Fact]
    public void Subtotal_IsUnitPriceTimesQuantity()
    {
        var item = new OrderItem(Guid.NewGuid(), 12.50m, 4);

        Assert.Equal(50m, item.Subtotal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveQuantity_Throws(int quantity)
    {
        Assert.Throws<DomainException>(() => new OrderItem(Guid.NewGuid(), 10m, quantity));
    }

    [Fact]
    public void Constructor_NegativePrice_Throws()
    {
        Assert.Throws<DomainException>(() => new OrderItem(Guid.NewGuid(), -1m, 1));
    }

    [Fact]
    public void Constructor_EmptyProduct_Throws()
    {
        Assert.Throws<DomainException>(() => new OrderItem(Guid.Empty, 10m, 1));
    }
}
