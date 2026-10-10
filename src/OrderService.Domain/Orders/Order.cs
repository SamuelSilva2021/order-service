using OrderService.Domain.Common;

namespace OrderService.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> items = [];

    private Order()
    {
        CustomerId = string.Empty;
        Currency = null!;
    }

    public Guid Id { get; private set; }

    public string CustomerId { get; private set; }

    public OrderStatus Status { get; private set; }

    public Currency Currency { get; private set; }

    public decimal Total { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => items;

    public bool HoldsStock => Status == OrderStatus.Confirmed;

    public static Order Place(string customerId, Currency currency, IEnumerable<OrderItem> items, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new DomainException("O cliente é obrigatório.");

        var orderItems = items.ToList();

        if (orderItems.Count == 0)
            throw new DomainException("O pedido deve ter ao menos um item.");

        if (orderItems.Select(item => item.ProductId).Distinct().Count() != orderItems.Count)
            throw new DomainException("O pedido não pode conter o mesmo produto mais de uma vez.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId.Trim(),
            Currency = currency,
            Status = OrderStatus.Placed,
            CreatedAt = createdAt,
            Total = orderItems.Sum(item => item.Subtotal)
        };

        order.items.AddRange(orderItems);

        return order;
    }

    public bool Confirm()
    {
        if (Status == OrderStatus.Confirmed) 
            return false;

        if (Status != OrderStatus.Placed)
            throw new InvalidOrderTransitionException(Id, Status, OrderStatus.Confirmed);

        Status = OrderStatus.Confirmed;
        return true;
    }

    public bool Cancel()
    {
        if (Status == OrderStatus.Canceled)
            return false;


        if (Status is not (OrderStatus.Placed or OrderStatus.Confirmed))
            throw new InvalidOrderTransitionException(Id, Status, OrderStatus.Canceled);

        Status = OrderStatus.Canceled;
        return true;
    }
}
