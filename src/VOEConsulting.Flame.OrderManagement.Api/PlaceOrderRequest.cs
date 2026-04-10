namespace VOEConsulting.Flame.OrderManagement.Api;

internal sealed record PlaceOrderRequest(
    string TenantId,
    string OrderId,
    string CustomerId,
    decimal TotalAmount = 0,
    string Currency = "USD",
    decimal? SubtotalAmount = null,
    decimal? DiscountAmount = null,
    decimal? TaxAmount = null);
