using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Cosmos clients with 429-aware retries and order/outbox persistence.
    /// The target container must use partition key path <c>/tenantId</c> (same partition for order + outbox batch).
    /// </summary>
    public static IServiceCollection AddOrderManagementCosmos(
        this IServiceCollection services,
        string connectionString,
        string databaseName,
        string ordersContainerName)
    {
        services.AddSingleton(_ =>
        {
            var options = new CosmosClientOptions
            {
                MaxRetryAttemptsOnRateLimitedRequests = 9,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            var client = new CosmosClient(connectionString, options);

            // Ensure core write-side resources exist so API/worker startup does not fail with 404.
            var database = client.CreateDatabaseIfNotExistsAsync(databaseName)
                .GetAwaiter()
                .GetResult()
                .Database;
            var ordersContainer = database.CreateContainerIfNotExistsAsync(
                    new ContainerProperties(ordersContainerName, "/tenantId"))
                .GetAwaiter()
                .GetResult()
                .Container;

            var ordersContainerProps = ordersContainer.ReadContainerAsync()
                .GetAwaiter()
                .GetResult()
                .Resource;
            var pkPaths = ordersContainerProps.PartitionKeyPaths ?? Array.Empty<string>();
            var hasExpectedPk =
                pkPaths.Count == 1
                && string.Equals(pkPaths[0], "/tenantId", StringComparison.Ordinal);
            if (!hasExpectedPk)
            {
                var actualPk = pkPaths.Count == 0 ? "<none>" : string.Join(", ", pkPaths);
                throw new InvalidOperationException(
                    $"Cosmos container '{ordersContainerName}' partition key paths are '{actualPk}'. " +
                    "Transactional batch requires exactly one partition key path: '/tenantId'. " +
                    "Recreate the container with /tenantId or point configuration to a compatible container.");
            }

            return client;
        });

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            return client.GetDatabase(databaseName).GetContainer(ordersContainerName);
        });

        services.AddSingleton<IOrderRepository, CosmosOrderRepository>();
        services.AddSingleton<IOutboxProcessedMarker, CosmosOutboxProcessedMarker>();
        return services;
    }

    /// <summary>
    /// Read model queries for <c>order-summaries</c> (partition key <c>/customerId</c>).
    /// </summary>
    public static IServiceCollection AddOrderSummaryQueries(
        this IServiceCollection services,
        string databaseName,
        string orderSummariesContainerName)
    {
        services.AddSingleton<IOrderSummaryQuery>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var container = client.GetDatabase(databaseName).GetContainer(orderSummariesContainerName);
            return new CosmosOrderSummaryQuery(container);
        });
        return services;
    }

    /// <summary>
    /// Ops list queries for <c>order-ops-index</c> (partition key <c>/opsPartitionKey</c> = <c>tenantId|status|yyyy-MM</c>).
    /// </summary>
    public static IServiceCollection AddOrderOpsQueries(
        this IServiceCollection services,
        string databaseName,
        string orderOpsContainerName)
    {
        services.AddSingleton<IOpsOrderQuery>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var container = client.GetDatabase(databaseName).GetContainer(orderOpsContainerName);
            return new CosmosOpsOrderQuery(container);
        });
        return services;
    }

    /// <summary>
    /// Runs the change feed processor for the orders container: dispatches outbox rows then marks them processed.
    /// Create a lease container (PK <c>/id</c>) if it does not exist.
    /// </summary>
    public static IServiceCollection AddOutboxChangeFeed(this IServiceCollection services, Action<OutboxChangeFeedOptions> configure)
    {
        services.Configure(configure);
        services.AddHostedService<OutboxChangeFeedHostedService>();
        return services;
    }

    /// <summary>
    /// Projects order writes into the <c>order-summaries</c> container (partition key <c>/customerId</c>).
    /// Uses a separate change feed processor name from the outbox dispatcher; same lease container is fine.
    /// </summary>
    public static IServiceCollection AddOrderSummaryProjector(this IServiceCollection services, Action<OrderSummaryProjectorOptions> configure)
    {
        services.Configure(configure);
        services.AddHostedService<OrderSummaryProjectorHostedService>();
        return services;
    }

    /// <summary>
    /// Projects orders into <c>order-ops-index</c> for tenant-scoped ops queries (filter by status in SQL).
    /// </summary>
    public static IServiceCollection AddOrderOpsProjector(this IServiceCollection services, Action<OpsOrderListProjectorOptions> configure)
    {
        services.Configure(configure);
        services.AddHostedService<OpsOrderListProjectorHostedService>();
        return services;
    }
}
