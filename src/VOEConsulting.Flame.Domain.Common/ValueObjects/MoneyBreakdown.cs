using CSharpFunctionalExtensions;

namespace VOEConsulting.Flame.Domain.Common.ValueObjects;

/// <summary>
/// Captures subtotal/discount/tax/total with currency-safe invariants.
/// </summary>
public sealed record MoneyBreakdown
{
    private MoneyBreakdown(Money subtotal, Money discount, Money tax, Money total)
    {
        Subtotal = subtotal;
        Discount = discount;
        Tax = tax;
        Total = total;
    }

    public Money Subtotal { get; }
    public Money Discount { get; }
    public Money Tax { get; }
    public Money Total { get; }

    public static Result<MoneyBreakdown, DomainError> Create(Money subtotal, Money discount, Money tax)
    {
        if (subtotal.Currency != discount.Currency || subtotal.Currency != tax.Currency)
            return Result.Failure<MoneyBreakdown, DomainError>(DomainError.BadRequest("Currency mismatch in money breakdown."));

        if (discount.Amount > subtotal.Amount)
            return Result.Failure<MoneyBreakdown, DomainError>(DomainError.BadRequest("Discount cannot be greater than subtotal."));

        var afterDiscount = subtotal.Subtract(discount);
        if (afterDiscount.IsFailure)
            return Result.Failure<MoneyBreakdown, DomainError>(afterDiscount.Error);

        var total = afterDiscount.Value.Add(tax);
        if (total.IsFailure)
            return Result.Failure<MoneyBreakdown, DomainError>(total.Error);

        return Result.Success<MoneyBreakdown, DomainError>(new MoneyBreakdown(subtotal, discount, tax, total.Value));
    }

    /// <summary>
    /// Backward-compatible factory: given a final total, creates a breakdown with tax=0 and discount=0.
    /// </summary>
    public static MoneyBreakdown FromTotal(Money total)
        => new(total, Money.Create(0, total.Currency).Value, Money.Create(0, total.Currency).Value, total);
}

