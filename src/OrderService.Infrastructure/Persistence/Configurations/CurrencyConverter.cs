using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrderService.Domain.Common;

namespace OrderService.Infrastructure.Persistence.Configurations;

public sealed class CurrencyConverter() : ValueConverter<Currency, string>(
    currency => currency.Code,
    code => Currency.Of(code));
