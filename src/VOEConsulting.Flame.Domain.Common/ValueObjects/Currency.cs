using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace VOEConsulting.Flame.Domain.Common.ValueObjects;

/// <summary>
/// ISO 4217-like currency code value object.
/// </summary>
public sealed record Currency
{
    private static readonly Regex CodeRegex = new("^[A-Z]{3}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private Currency(string code) => Code = code;

    public string Code { get; }

    public static Result<Currency, DomainError> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Currency, DomainError>(DomainError.BadRequest("Currency is required."));

        var normalized = code.Trim().ToUpperInvariant();
        if (!CodeRegex.IsMatch(normalized))
            return Result.Failure<Currency, DomainError>(DomainError.BadRequest("Currency must be a 3-letter ISO code (e.g., USD, EUR)."));

        return Result.Success<Currency, DomainError>(new Currency(normalized));
    }

    public override string ToString() => Code;

    public static Currency USD => new("USD");
    public static Currency EUR => new("EUR");
    public static Currency GBP => new("GBP");
}

