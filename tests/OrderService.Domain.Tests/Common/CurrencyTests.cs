using OrderService.Domain.Common;

namespace OrderService.Domain.Tests.Common;

public class CurrencyTests
{
    [Fact]
    public void Of_NormalizesToUpperCase()
    {
        Assert.Equal("BRL", Currency.Of(" brl ").Code);
    }

    [Fact]
    public void Of_SameCode_AreEqual()
    {
        Assert.Equal(Currency.Of("usd"), Currency.Of("USD"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("BR")]
    [InlineData("BRLL")]
    [InlineData("B1L")]
    public void Of_InvalidCode_Throws(string? code)
    {
        Assert.Throws<DomainException>(() => Currency.Of(code));
    }
}
