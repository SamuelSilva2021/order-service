using OrderService.Domain.Common;
using OrderService.Domain.Orders;

namespace OrderService.Domain.Tests.Orders;

public class OrderTests
{
    private static readonly Currency Brl = Currency.Of("BRL");

    private static Order PlaceOrder(params OrderItem[] items) =>
        Order.Place("customer-1", Brl, items.Length == 0 ? [new OrderItem(Guid.NewGuid(), 10m, 1)] : items, DateTimeOffset.UtcNow);

    [Fact]
    public void Place_WithItems_StartsPlacedWithSummedTotal()
    {
        var order = PlaceOrder(new OrderItem(Guid.NewGuid(), 10m, 2), new OrderItem(Guid.NewGuid(), 5.5m, 3));

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal(36.5m, order.Total);
        Assert.Equal(2, order.Items.Count);
        Assert.NotEqual(Guid.Empty, order.Id);
    }

    [Fact]
    public void Place_WithoutItems_Throws()
    {
        Assert.Throws<DomainException>(() => Order.Place("customer-1", Brl, [], DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Place_WithDuplicatedProduct_Throws()
    {
        var productId = Guid.NewGuid();

        Assert.Throws<DomainException>(() =>
            Order.Place("customer-1", Brl, [new OrderItem(productId, 1m, 1), new OrderItem(productId, 1m, 2)], DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Place_WithoutCustomer_Throws(string customerId)
    {
        Assert.Throws<DomainException>(() =>
            Order.Place(customerId, Brl, [new OrderItem(Guid.NewGuid(), 1m, 1)], DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Confirm_FromPlaced_ChangesStatusAndReportsChange()
    {
        var order = PlaceOrder();

        var changed = order.Confirm();

        Assert.True(changed);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.True(order.HoldsStock);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_IsIdempotent()
    {
        var order = PlaceOrder();
        order.Confirm();

        var changed = order.Confirm();

        Assert.False(changed);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_WhenCanceled_Throws()
    {
        var order = PlaceOrder();
        order.Cancel();

        Assert.Throws<InvalidOrderTransitionException>(() => order.Confirm());
    }

    [Fact]
    public void Cancel_FromPlaced_CancelsWithoutHoldingStock()
    {
        var order = PlaceOrder();

        var changed = order.Cancel();

        Assert.True(changed);
        Assert.Equal(OrderStatus.Canceled, order.Status);
        Assert.False(order.HoldsStock);
    }

    [Fact]
    public void Cancel_FromConfirmed_Cancels()
    {
        var order = PlaceOrder();
        order.Confirm();

        var changed = order.Cancel();

        Assert.True(changed);
        Assert.Equal(OrderStatus.Canceled, order.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCanceled_IsIdempotent()
    {
        var order = PlaceOrder();
        order.Cancel();

        var changed = order.Cancel();

        Assert.False(changed);
        Assert.Equal(OrderStatus.Canceled, order.Status);
    }
}
