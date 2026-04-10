using Microsoft.Azure.Cosmos;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

/// <summary>
/// Shared synchronous bootstrap for change feed processors: database + containers with fixed partition key paths used by this solution.
/// </summary>
internal static class ChangeFeedProcessorBootstrap
{
    private const string OrdersPartitionKeyPath = "/tenantId";
    private const string LeasePartitionKeyPath = "/id";

    public static Database EnsureDatabase(CosmosClient client, string databaseName)
    {
        return client.CreateDatabaseIfNotExistsAsync(databaseName)
            .GetAwaiter()
            .GetResult()
            .Database;
    }

    public static Container EnsureOrdersMonitoredContainer(Database database, string containerName)
    {
        return database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerName, OrdersPartitionKeyPath))
            .GetAwaiter()
            .GetResult()
            .Container;
    }

    public static Container EnsureLeaseContainer(Database database, string containerName)
    {
        return database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerName, LeasePartitionKeyPath))
            .GetAwaiter()
            .GetResult()
            .Container;
    }

    public static Container EnsureContainer(Database database, string containerName, string partitionKeyPath)
    {
        return database.CreateContainerIfNotExistsAsync(new ContainerProperties(containerName, partitionKeyPath))
            .GetAwaiter()
            .GetResult()
            .Container;
    }
}
