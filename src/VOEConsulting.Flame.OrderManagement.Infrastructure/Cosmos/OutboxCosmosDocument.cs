using System.Text.Json.Serialization;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

internal sealed class OutboxCosmosDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = "";

    [JsonPropertyName("entityType")]
    public string EntityType { get; set; } = "Outbox";

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = "";

    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = "";

    [JsonPropertyName("payloadJson")]
    public string PayloadJson { get; set; } = "";

    [JsonPropertyName("processed")]
    public bool Processed { get; set; }

    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
