using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

public sealed class OrderSummaryProjectorHostedService : IHostedService
{
    private const string SummariesPartitionKeyPath = "/customerId";

    private readonly CosmosClient _cosmosClient;
    private readonly OrderSummaryProjectorOptions _options;
    private readonly ILogger<OrderSummaryProjectorHostedService> _logger;
    private ChangeFeedProcessor? _processor;

    public OrderSummaryProjectorHostedService(
        CosmosClient cosmosClient,
        IOptions<OrderSummaryProjectorOptions> options,
        ILogger<OrderSummaryProjectorHostedService> logger)
    {
        _cosmosClient = cosmosClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ThrowIfOptionsInvalid();

        var database = ChangeFeedProcessorBootstrap.EnsureDatabase(_cosmosClient, _options.DatabaseName);
        var monitored = ChangeFeedProcessorBootstrap.EnsureOrdersMonitoredContainer(database, _options.OrdersContainerName);
        var lease = ChangeFeedProcessorBootstrap.EnsureLeaseContainer(database, _options.LeaseContainerName);
        var summaries = ChangeFeedProcessorBootstrap.EnsureContainer(
            database,
            _options.SummariesContainerName,
            SummariesPartitionKeyPath);

        _processor = monitored
            .GetChangeFeedProcessorBuilder<ChangeFeedItem>(_options.ProcessorName, (ctx, changes, ct) =>
                ProjectSummariesAsync(summaries, changes, ct))
            .WithInstanceName(_options.InstanceName)
            .WithLeaseContainer(lease)
            .Build();

        return _processor.StartAsync();
    }

    private void ThrowIfOptionsInvalid()
    {
        if (string.IsNullOrWhiteSpace(_options.DatabaseName)
            || string.IsNullOrWhiteSpace(_options.OrdersContainerName)
            || string.IsNullOrWhiteSpace(_options.SummariesContainerName)
            || string.IsNullOrWhiteSpace(_options.LeaseContainerName))
        {
            throw new InvalidOperationException(
                "OrderSummaryProjectorOptions.DatabaseName, OrdersContainerName, SummariesContainerName, and LeaseContainerName must be set.");
        }
    }

    private Task ProjectSummariesAsync(
        Container summaries,
        IReadOnlyCollection<ChangeFeedItem> changes,
        CancellationToken cancellationToken)
    {
        var spec = new ChangeFeedItemHandlerSpec(
            EntityType: ChangeFeedEntityTypes.Order,
            IsValidItem: item => !string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.CustomerId),
            InvalidItemLogMessage: "Skipping Order change with missing id or customerId.",
            HandleItemAsync: (item, ct) => UpsertSummaryAsync(summaries, item, ct));

        return ChangeFeedItemBatchProcessor.ProcessAsync(changes, spec, _logger, cancellationToken);
    }

    private static Task UpsertSummaryAsync(Container summaries, ChangeFeedItem item, CancellationToken cancellationToken)
    {
        var doc = new OrderSummaryCosmosDocument
        {
            Id = item.Id,
            CustomerId = item.CustomerId!,
            TenantId = item.TenantId,
            Status = item.Status ?? "",
            PlacedAt = item.PlacedAt,
            UpdatedAt = item.UpdatedAt ?? DateTimeOffset.UtcNow
        };

        return summaries.UpsertItemAsync(doc, new PartitionKey(doc.CustomerId), cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
            await _processor.StopAsync();
    }
}
