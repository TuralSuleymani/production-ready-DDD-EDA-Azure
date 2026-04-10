using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Hosting;

public static class OrderManagementHostingServiceCollectionExtensions
{
    public static IServiceCollection AddOrderManagementPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        OrderManagementConfiguration.CosmosConnectionOptions cosmos)
    {
        services.AddOrderManagementCosmos(cosmos.ConnectionString, cosmos.DatabaseName, cosmos.OrdersContainer);
        services.AddIntegrationEventPublishing(configuration);
        services.AddIntegrationAuditLog(configuration, cosmos.DatabaseName);
        return services;
    }

    public static IServiceCollection AddOrderManagementChangeFeedProjectors(
        this IServiceCollection services,
        IConfiguration configuration,
        OrderManagementConfiguration.CosmosConnectionOptions cosmos)
    {
        services.AddOutboxChangeFeed(o =>
        {
            o.DatabaseName = cosmos.DatabaseName;
            o.OrdersContainerName = cosmos.OrdersContainer;
            o.LeaseContainerName = cosmos.LeaseContainer;
            o.ProcessorName = configuration["Cosmos:ChangeFeed:ProcessorName"] ?? "outbox-dispatcher";
            o.InstanceName = configuration["Cosmos:ChangeFeed:InstanceName"] ?? Environment.MachineName;
        });

        services.AddOrderSummaryProjector(o =>
        {
            o.DatabaseName = cosmos.DatabaseName;
            o.OrdersContainerName = cosmos.OrdersContainer;
            o.SummariesContainerName = cosmos.OrderSummariesContainer;
            o.LeaseContainerName = cosmos.LeaseContainer;
            o.ProcessorName = configuration["Cosmos:OrderSummaryFeed:ProcessorName"] ?? "order-summary-projector";
            o.InstanceName = configuration["Cosmos:OrderSummaryFeed:InstanceName"] ?? Environment.MachineName;
        });

        services.AddOrderOpsProjector(o =>
        {
            o.DatabaseName = cosmos.DatabaseName;
            o.OrdersContainerName = cosmos.OrdersContainer;
            o.OpsContainerName = cosmos.OrderOpsContainer;
            o.LeaseContainerName = cosmos.LeaseContainer;
            o.ProcessorName = configuration["Cosmos:OpsOrderFeed:ProcessorName"] ?? "ops-order-list-projector";
            o.InstanceName = configuration["Cosmos:OpsOrderFeed:InstanceName"] ?? Environment.MachineName;
        });

        return services;
    }
}
