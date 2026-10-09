using OrderService.Domain.Common;
using OrderService.Domain.Orders;

namespace OrderService.Domain.Products;

public sealed class Product
{
    private Product()
    {
        Name = string.Empty;
        Currency = null!;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public decimal UnitPrice { get; private set; }

    public Currency Currency { get; private set; }

    public int AvailableQuantity { get; private set; }

    public static Product Create(string name, decimal unitPrice, Currency currency, int availableQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("O nome do produto é obrigatório.");
        }

        if (unitPrice <= 0)
        {
            throw new DomainException("O preço unitário deve ser maior que zero.");
        }

        if (availableQuantity < 0)
        {
            throw new DomainException("A quantidade disponível não pode ser negativa.");
        }

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            UnitPrice = unitPrice,
            Currency = currency,
            AvailableQuantity = availableQuantity
        };
    }

    public OrderItem ToOrderItem(int quantity, Currency orderCurrency)
    {
        if (orderCurrency != Currency)
        {
            throw new DomainException($"O produto {Id} é precificado em {Currency}, e não em {orderCurrency}.");
        }

        EnsureCanFulfill(quantity);

        return new OrderItem(Id, UnitPrice, quantity);
    }

    public void Reserve(int quantity)
    {
        EnsureCanFulfill(quantity);
        AvailableQuantity -= quantity;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("A quantidade deve ser maior que zero.");
        }

        AvailableQuantity += quantity;
    }

    private void EnsureCanFulfill(int quantity)
    {
        if (quantity > AvailableQuantity)
        {
            throw new InsufficientStockException(Id, quantity, AvailableQuantity);
        }
    }
}
