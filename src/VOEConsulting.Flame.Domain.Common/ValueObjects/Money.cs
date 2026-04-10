using CSharpFunctionalExtensions;

namespace VOEConsulting.Flame.Domain.Common.ValueObjects;

/// <summary>
/// Money value object. All arithmetic is currency-safe (same currency required).
/// </summary>
public sealed record Money
{
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public Currency Currency { get; }

    public static Result<Money, DomainError> Create(decimal amount, string currencyCode)
    {
        if (amount < 0)
            return Result.Failure<Money, DomainError>(DomainError.BadRequest("Amount must be greater than or equal to 0."));

        var currencyResult = Currency.Create(currencyCode);
        if (currencyResult.IsFailure)
            return Result.Failure<Money, DomainError>(currencyResult.Error);

        return Result.Success<Money, DomainError>(new Money(amount, currencyResult.Value));
    }

    public static Result<Money, DomainError> Create(decimal amount, Currency currency)
    {
        if (amount < 0)
            return Result.Failure<Money, DomainError>(DomainError.BadRequest("Amount must be greater than or equal to 0."));
        return Result.Success<Money, DomainError>(new Money(amount, currency));
    }

    public Result<Money, DomainError> Add(Money other)
    {
        if (other.Currency != Currency)
            return Result.Failure<Money, DomainError>(DomainError.BadRequest($"Currency mismatch: {Currency.Code} vs {other.Currency.Code}."));
        return Result.Success<Money, DomainError>(new Money(Amount + other.Amount, Currency));
    }

    public Result<Money, DomainError> Subtract(Money other)
    {
        if (other.Currency != Currency)
            return Result.Failure<Money, DomainError>(DomainError.BadRequest($"Currency mismatch: {Currency.Code} vs {other.Currency.Code}."));

        var result = Amount - other.Amount;
        if (result < 0)
            return Result.Failure<Money, DomainError>(DomainError.BadRequest("Money result cannot be negative."));

        return Result.Success<Money, DomainError>(new Money(result, Currency));
    }

    public Result<Money, DomainError> Multiply(decimal factor)
    {
        if (factor < 0)
            return Result.Failure<Money, DomainError>(DomainError.BadRequest("Factor must be greater than or equal to 0."));

        return Result.Success<Money, DomainError>(new Money(Amount * factor, Currency));
    }

    public override string ToString() => $"{Amount} {Currency.Code}";
}

