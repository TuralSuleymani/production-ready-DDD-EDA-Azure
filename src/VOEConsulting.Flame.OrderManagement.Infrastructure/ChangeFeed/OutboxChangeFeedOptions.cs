namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

public sealed class OutboxChangeFeedOptions
{
    public string DatabaseName { get; set; } = "";
    public string OrdersContainerName { get; set; } = "";
    public string LeaseContainerName { get; set; } = "leases";
    public string ProcessorName { get; set; } = "outbox-dispatcher";
    public string InstanceName { get; set; } = Environment.MachineName;
}
