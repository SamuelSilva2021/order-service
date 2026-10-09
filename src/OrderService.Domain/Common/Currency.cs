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
            throw new DomainException("A moeda deve ser um código ISO 4217 de três letras.");
        }

        return new Currency(normalized);
    }

    public override string ToString() => Code;
}
