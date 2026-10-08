using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Orders;

namespace OrderService.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);

        builder.Property(order => order.CustomerId).HasMaxLength(64).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(order => order.Currency).HasConversion<CurrencyConverter>().HasMaxLength(3).IsRequired();
        builder.Property(order => order.Total).HasPrecision(18, 2);
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property<uint>("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        builder.Ignore(order => order.HoldsStock);

        builder.OwnsMany(order => order.Items, items =>
        {
            items.ToTable("order_items");
            items.WithOwner().HasForeignKey("OrderId");
            items.HasKey("OrderId", nameof(OrderItem.ProductId));
            items.Property(item => item.UnitPrice).HasPrecision(18, 2);
            items.Ignore(item => item.Subtotal);
        });

        builder.HasIndex(order => new { order.CustomerId, order.CreatedAt });
        builder.HasIndex(order => new { order.Status, order.CreatedAt });
        builder.HasIndex(order => order.CreatedAt);
    }
}
