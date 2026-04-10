using CSharpFunctionalExtensions;

namespace VOEConsulting.Flame.Domain.Common.ValueObjects;

/// <summary>
/// Percentage value object in range [0..100].
/// </summary>
public sealed record Percentage
{
    private Percentage(decimal value) => Value = value;

    public decimal Value { get; }

    public static Result<Percentage, DomainError> Create(decimal value)
    {
        if (value < 0 || value > 100)
            return Result.Failure<Percentage, DomainError>(DomainError.BadRequest("Percentage must be between 0 and 100."));

        return Result.Success<Percentage, DomainError>(new Percentage(value));
    }

    public decimal ToFactor() => Value / 100m;
}

