using Newtonsoft.Json;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

/// <summary>
/// Loose shape for both Order and Outbox documents in the monitored container so the feed can filter without failing deserialization.
/// </summary>
internal sealed class ChangeFeedItem
{
    [JsonProperty("id")]
    public string Id { get; set; } = "";

    [JsonProperty("tenantId")]
    public string TenantId { get; set; } = "";

    [JsonProperty("entityType")]
    public string EntityType { get; set; } = "";

    [JsonProperty("processed")]
    public bool? Processed { get; set; }

    [JsonProperty("orderId")]
    public string? OrderId { get; set; }

    [JsonProperty("eventType")]
    public string? EventType { get; set; }

    [JsonProperty("payloadJson")]
    public string? PayloadJson { get; set; }

    [JsonProperty("customerId")]
    public string? CustomerId { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("placedAt")]
    public DateTimeOffset? PlacedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
