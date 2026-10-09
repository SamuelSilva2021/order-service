using OrderService.Domain.Orders;

namespace OrderService.Domain.Tests.Orders;

public class InvalidOrderTransitionExceptionTests
{
    [Fact]
    public void Message_UsesPortugueseStatusNames()
    {
        var orderId = Guid.NewGuid();

        var exception = new InvalidOrderTransitionException(orderId, OrderStatus.Canceled, OrderStatus.Confirmed);

        Assert.Equal($"O pedido {orderId} não pode passar de Cancelado para Confirmado.", exception.Message);
    }

    [Theory]
    [InlineData(OrderStatus.Draft, "Rascunho")]
    [InlineData(OrderStatus.Placed, "Registrado")]
    [InlineData(OrderStatus.Confirmed, "Confirmado")]
    [InlineData(OrderStatus.Canceled, "Cancelado")]
    public void ToDisplayName_TranslatesEveryStatus(OrderStatus status, string expected)
    {
        Assert.Equal(expected, status.ToDisplayName());
    }

    [Fact]
    public void ToDisplayName_CoversAllEnumValues()
    {
        foreach (var status in Enum.GetValues<OrderStatus>())
        {
            Assert.NotEqual(status.ToString(), status.ToDisplayName());
        }
    }
}
