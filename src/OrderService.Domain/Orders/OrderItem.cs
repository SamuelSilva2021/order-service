using OrderService.Domain.Common;

namespace OrderService.Domain.Orders;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    public OrderItem(Guid productId, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("O item do pedido deve referenciar um produto.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("O preço unitário não pode ser negativo.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("A quantidade deve ser maior que zero.");
        }

        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal Subtotal => UnitPrice * Quantity;
}
