namespace OrderService.Domain.Orders;

public static class OrderStatusExtensions
{
    public static string ToDisplayName(this OrderStatus status) => status switch
    {
        OrderStatus.Draft => "Rascunho",
        OrderStatus.Placed => "Registrado",
        OrderStatus.Confirmed => "Confirmado",
        OrderStatus.Canceled => "Cancelado",
        _ => status.ToString()
    };
}
