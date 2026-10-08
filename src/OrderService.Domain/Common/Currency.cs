namespace OrderService.Domain.Common;

public sealed record Currency
{
    private Currency(string code) => Code = code;

    public string Code { get; }

    public static Currency Of(string? code)
    {
        var normalized = code?.Trim().ToUpperInvariant();

        if (normalized is not { Length: 3 } || !normalized.All(char.IsAsciiLetter))
        {
            throw new DomainException("Currency must be a three-letter ISO 4217 code.");
        }

        return new Currency(normalized);
    }

    public override string ToString() => Code;
}
