using MediatR;

namespace VOEConsulting.Flame.OrderManagement.Api;

internal static class OrderApiEndpoints
{
    public static WebApplication MapOrderManagementEndpoints(this WebApplication app)
    {
        app.MapGet("/customers/{customerId}/orders", async (
            string customerId,
            IOrderSummaryQuery query,
            CancellationToken ct) =>
        {
            var list = await query.ListByCustomerAsync(customerId, ct);
            return Results.Ok(list);
        });

        app.MapGet("/customers/{customerId}/orders/{orderId}", async (
            string customerId,
            string orderId,
            IOrderSummaryQuery query,
            CancellationToken ct) =>
        {
            var item = await query.GetAsync(customerId, orderId, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        app.MapPost("/orders", async (PlaceOrderRequest body, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new PlaceOrderCommand(
                    body.TenantId,
                    body.OrderId,
                    body.CustomerId,
                    body.TotalAmount,
                    body.Currency,
                    body.SubtotalAmount,
                    body.DiscountAmount,
                    body.TaxAmount),
                ct);
            if (result.IsFailure)
                return DomainErrorHttp.Map(result.Error);

            return Results.Created(
                $"/customers/{Uri.EscapeDataString(body.CustomerId)}/orders/{Uri.EscapeDataString(body.OrderId)}",
                new
                {
                    body.TenantId,
                    body.OrderId,
                    body.CustomerId,
                    body.TotalAmount,
                    body.Currency,
                    body.SubtotalAmount,
                    body.DiscountAmount,
                    body.TaxAmount
                });
        });

        app.MapPost("/tenants/{tenantId}/orders/{orderId}/cancel", async (
            string tenantId,
            string orderId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CancelOrderCommand(tenantId, orderId), ct);
            if (result.IsFailure)
                return DomainErrorHttp.Map(result.Error);
            return Results.NoContent();
        });

        app.MapPost("/tenants/{tenantId}/orders/{orderId}/ship", async (
            string tenantId,
            string orderId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new MarkShippedCommand(tenantId, orderId), ct);
            if (result.IsFailure)
                return DomainErrorHttp.Map(result.Error);
            return Results.NoContent();
        });

        app.MapGet("/tenants/{tenantId}/ops/orders", async (
            string tenantId,
            string status,
            string yearMonth,
            IOpsOrderQuery query,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(yearMonth))
                return Results.BadRequest(new { error = "Query parameters status and yearMonth (yyyy-MM) are required." });

            var list = await query.ListByStatusMonthAsync(tenantId, status, yearMonth, ct);
            return Results.Ok(list);
        });

        return app;
    }
}
