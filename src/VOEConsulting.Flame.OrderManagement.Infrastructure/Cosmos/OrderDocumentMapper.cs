using VOEConsulting.Flame.Domain.Common;
using VOEConsulting.Flame.Domain.Common.ValueObjects;
using VOEConsulting.Flame.OrderManagement.Domain.Orders;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

internal static class OrderDocumentMapper
{
    public static OrderCosmosDocument ToDocument(string tenantId, Order order)
    {
        return new OrderCosmosDocument
        {
            Id = order.OrderId,
            TenantId = tenantId,
            EntityType = "Order",
            AggregateId = order.Id.Value.ToString("D"),
            CustomerId = order.CustomerId,
            Status = order.Status.Name,
            TotalAmount = order.Pricing.Total.Amount,
            SubtotalAmount = order.Pricing.Subtotal.Amount,
            DiscountAmount = order.Pricing.Discount.Amount,
            TaxAmount = order.Pricing.Tax.Amount,
            Currency = order.Pricing.Total.Currency.Code,
            PlacedAt = order.CreatedAtUtc,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public static Order ToDomain(OrderCosmosDocument doc)
    {
        if (!string.Equals(doc.EntityType, "Order", StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected Order document, got '{doc.EntityType}'.");

        if (string.IsNullOrWhiteSpace(doc.AggregateId))
            throw new InvalidOperationException("Order document is missing aggregateId.");

        var aggregateId = Id<Order>.FromGuid(Guid.Parse(doc.AggregateId));
        var status = OrderStatus.FromName(doc.Status, ignoreCase: true);
        var placedAt = doc.PlacedAt ?? DateTimeOffset.UtcNow;
        var updatedAt = doc.UpdatedAt ?? placedAt;

        var currencyResult = Currency.Create(doc.Currency);
        if (currencyResult.IsFailure)
            throw new InvalidOperationException($"Invalid currency data on Order document: {currencyResult.Error.ErrorMessage}");

        var currency = currencyResult.Value;

        var total = Money.Create(doc.TotalAmount, currency);
        if (total.IsFailure)
            throw new InvalidOperationException($"Invalid total money data on Order document: {total.Error.ErrorMessage}");

        var subtotalAmount = doc.SubtotalAmount ?? doc.TotalAmount;
        var discountAmount = doc.DiscountAmount ?? 0m;
        var taxAmount = doc.TaxAmount ?? 0m;

        var subtotal = Money.Create(subtotalAmount, currency);
        if (subtotal.IsFailure)
            throw new InvalidOperationException($"Invalid subtotal money data on Order document: {subtotal.Error.ErrorMessage}");

        var discount = Money.Create(discountAmount, currency);
        if (discount.IsFailure)
            throw new InvalidOperationException($"Invalid discount money data on Order document: {discount.Error.ErrorMessage}");

        var tax = Money.Create(taxAmount, currency);
        if (tax.IsFailure)
            throw new InvalidOperationException($"Invalid tax money data on Order document: {tax.Error.ErrorMessage}");

        var pricing = MoneyBreakdown.Create(subtotal.Value, discount.Value, tax.Value);
        if (pricing.IsFailure)
            throw new InvalidOperationException($"Invalid pricing breakdown on Order document: {pricing.Error.ErrorMessage}");

        if (pricing.Value.Total.Amount != total.Value.Amount)
            throw new InvalidOperationException("Order document totalAmount does not match subtotalAmount - discountAmount + taxAmount.");

        return Order.Rehydrate(
            aggregateId,
            doc.Id,
            doc.CustomerId,
            pricing.Value,
            status,
            placedAt,
            updatedAt);
    }
}
