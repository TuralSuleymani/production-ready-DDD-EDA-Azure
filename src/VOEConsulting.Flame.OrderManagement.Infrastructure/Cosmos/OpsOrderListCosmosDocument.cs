using Newtonsoft.Json;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

internal sealed class OpsOrderListCosmosDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = "";

    /// <summary>Partition key value: <c>tenantId|status|yyyy-MM</c>.</summary>
    [JsonProperty("opsPartitionKey")]
    public string OpsPartitionKey { get; set; } = "";

    [JsonProperty("tenantId")]
    public string TenantId { get; set; } = "";

    [JsonProperty("customerId")]
    public string CustomerId { get; set; } = "";

    [JsonProperty("status")]
    public string Status { get; set; } = "";

    [JsonProperty("yearMonth")]
    public string YearMonth { get; set; } = "";

    [JsonProperty("entityType")]
    public string EntityType { get; set; } = "OpsOrderList";

    [JsonProperty("placedAt")]
    public DateTimeOffset PlacedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}
