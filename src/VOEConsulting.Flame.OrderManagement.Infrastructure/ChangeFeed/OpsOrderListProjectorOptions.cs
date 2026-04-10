namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

public sealed class OpsOrderListProjectorOptions
{
    public string DatabaseName { get; set; } = "";
    public string OrdersContainerName { get; set; } = "";
    public string OpsContainerName { get; set; } = "order-ops-index";
    public string LeaseContainerName { get; set; } = "leases";
    public string ProcessorName { get; set; } = "ops-order-list-projector";
    public string InstanceName { get; set; } = Environment.MachineName;
}
