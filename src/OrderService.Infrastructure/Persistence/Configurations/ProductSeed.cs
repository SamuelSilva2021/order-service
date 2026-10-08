using OrderService.Domain.Common;

namespace OrderService.Infrastructure.Persistence.Configurations;

public static class ProductSeed
{
    public static readonly Guid Keyboard = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Mouse = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Monitor = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Headset = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Webcam = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static object[] All() =>
    [
        Row(Keyboard, "Mechanical Keyboard", 349.90m, 50),
        Row(Mouse, "Wireless Mouse", 129.90m, 100),
        Row(Monitor, "27-inch Monitor", 1599.00m, 15),
        Row(Headset, "Noise Cancelling Headset", 799.50m, 30),
        Row(Webcam, "Full HD Webcam", 289.00m, 0)
    ];

    private static object Row(Guid id, string name, decimal unitPrice, int availableQuantity) =>
        new { Id = id, Name = name, UnitPrice = unitPrice, Currency = Currency.Of("BRL"), AvailableQuantity = availableQuantity };
}
