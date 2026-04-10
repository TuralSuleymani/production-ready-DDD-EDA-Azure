namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

public sealed class OrderSummaryProjectorOptions
{
    public string DatabaseName { get; set; } = "";
    public string OrdersContainerName { get; set; } = "";
    public string SummariesContainerName { get; set; } = "order-summaries";
    public string LeaseContainerName { get; set; } = "leases";
    public string ProcessorName { get; set; } = "order-summary-projector";
    public string InstanceName { get; set; } = Environment.MachineName;
}
