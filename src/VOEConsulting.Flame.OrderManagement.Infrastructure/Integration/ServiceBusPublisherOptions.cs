namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Integration;

public sealed class ServiceBusPublisherOptions
{
    public const string SectionName = "ServiceBus";

    /// <summary>Azure Service Bus connection string (omit to use logging publisher only).</summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>Queue or topic name for integration messages.</summary>
    public string QueueOrTopicName { get; set; } = "orders-integration";
}
