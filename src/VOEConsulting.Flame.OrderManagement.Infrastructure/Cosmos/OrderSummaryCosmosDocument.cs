using Newtonsoft.Json;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

/// <summary>Customer-centric read model (partition key <c>/customerId</c>).</summary>
internal sealed class OrderSummaryCosmosDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = "";

    [JsonProperty("customerId")]
    public string CustomerId { get; set; } = "";

    [JsonProperty("tenantId")]
    public string TenantId { get; set; } = "";

    [JsonProperty("status")]
    public string Status { get; set; } = "";

    [JsonProperty("entityType")]
    public string EntityType { get; set; } = "OrderSummary";

    [JsonProperty("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonProperty("placedAt")]
    public DateTimeOffset? PlacedAt { get; set; }
}
