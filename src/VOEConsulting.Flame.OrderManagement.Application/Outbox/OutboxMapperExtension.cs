using System.Text.Json;
using VOEConsulting.Flame.OrderManagement.Application.Integration;

namespace VOEConsulting.Flame.OrderManagement.Application.Outbox;

public static class OutboxMapperExtension
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<OutboxRecord> ToOutboxRecords(
        this IEnumerable<IDomainEvent> events,
        string tenantId,
        string orderId)
    {
        return events.Select(e =>
        {
            var (eventType, payload) = OrderIntegrationEventMapper.Map(e);
            return new OutboxRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                TenantId = tenantId,
                OrderId = orderId,
                EventType = eventType,
                PayloadJson = JsonSerializer.Serialize(payload, payload.GetType(), JsonOptions),
                Metadata = null,
                Processed = false
            };
        }).ToList();
    }
}
