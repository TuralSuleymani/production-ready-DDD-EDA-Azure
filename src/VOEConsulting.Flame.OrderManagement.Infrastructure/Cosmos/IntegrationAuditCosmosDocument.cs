using Newtonsoft.Json;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

internal sealed class IntegrationAuditCosmosDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = "";

    [JsonProperty("tenantId")]
    public string TenantId { get; set; } = "";

    [JsonProperty("orderId")]
    public string OrderId { get; set; } = "";

    [JsonProperty("outboxItemId")]
    public string OutboxItemId { get; set; } = "";

    [JsonProperty("eventType")]
    public string EventType { get; set; } = "";

    [JsonProperty("payloadJson")]
    public string PayloadJson { get; set; } = "";

    [JsonProperty("publishedAtUtc")]
    public DateTimeOffset PublishedAtUtc { get; set; }

    [JsonProperty("entityType")]
    public string EntityType { get; set; } = "IntegrationAudit";
}
