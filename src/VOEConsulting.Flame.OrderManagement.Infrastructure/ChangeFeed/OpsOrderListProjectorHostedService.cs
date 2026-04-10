using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

public sealed class OpsOrderListProjectorHostedService : IHostedService
{
    private const string OpsPartitionKeyPath = "/opsPartitionKey";

    private readonly CosmosClient _cosmosClient;
    private readonly OpsOrderListProjectorOptions _options;
    private readonly ILogger<OpsOrderListProjectorHostedService> _logger;
    private ChangeFeedProcessor? _processor;

    public OpsOrderListProjectorHostedService(
        CosmosClient cosmosClient,
        IOptions<OpsOrderListProjectorOptions> options,
        ILogger<OpsOrderListProjectorHostedService> logger)
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
        var ops = ChangeFeedProcessorBootstrap.EnsureContainer(database, _options.OpsContainerName, OpsPartitionKeyPath);

        _processor = monitored
            .GetChangeFeedProcessorBuilder<ChangeFeedItem>(_options.ProcessorName, (ctx, changes, ct) =>
                ProjectOpsIndexAsync(ops, changes, ct))
            .WithInstanceName(_options.InstanceName)
            .WithLeaseContainer(lease)
            .Build();

        return _processor.StartAsync();
    }

    private void ThrowIfOptionsInvalid()
    {
        if (string.IsNullOrWhiteSpace(_options.DatabaseName)
            || string.IsNullOrWhiteSpace(_options.OrdersContainerName)
            || string.IsNullOrWhiteSpace(_options.OpsContainerName)
            || string.IsNullOrWhiteSpace(_options.LeaseContainerName))
        {
            throw new InvalidOperationException(
                "OpsOrderListProjectorOptions.DatabaseName, OrdersContainerName, OpsContainerName, and LeaseContainerName must be set.");
        }
    }

    private Task ProjectOpsIndexAsync(
        Container ops,
        IReadOnlyCollection<ChangeFeedItem> changes,
        CancellationToken cancellationToken)
    {
        var spec = new ChangeFeedItemHandlerSpec(
            EntityType: ChangeFeedEntityTypes.Order,
            IsValidItem: item =>
                !string.IsNullOrEmpty(item.Id)
                && !string.IsNullOrEmpty(item.TenantId)
                && !string.IsNullOrEmpty(item.CustomerId),
            InvalidItemLogMessage: "Skipping Order change with missing id, tenantId, or customerId.",
            HandleItemAsync: (item, ct) => UpsertOpsRowAsync(ops, item, ct));

        return ChangeFeedItemBatchProcessor.ProcessAsync(changes, spec, _logger, cancellationToken);
    }

    private async Task UpsertOpsRowAsync(Container ops, ChangeFeedItem item, CancellationToken cancellationToken)
    {
        var placed = item.PlacedAt ?? DateTimeOffset.UtcNow;
        var updated = item.UpdatedAt ?? DateTimeOffset.UtcNow;
        var status = item.Status ?? "";
        var yearMonth = updated.UtcDateTime.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
        var newPk = OpsPartitionKeyHelper.Build(item.TenantId!, status, updated);

        await RemoveStaleOpsRowsAsync(ops, item.Id, newPk, cancellationToken).ConfigureAwait(false);

        var doc = new OpsOrderListCosmosDocument
        {
            Id = item.Id,
            OpsPartitionKey = newPk,
            TenantId = item.TenantId!,
            CustomerId = item.CustomerId!,
            Status = status,
            YearMonth = yearMonth,
            PlacedAt = placed,
            UpdatedAt = updated
        };

        await ops.UpsertItemAsync(doc, new PartitionKey(newPk), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task RemoveStaleOpsRowsAsync(
        Container ops,
        string orderId,
        string newPartitionKey,
        CancellationToken cancellationToken)
    {
        var sql = new QueryDefinition(
                "SELECT c.id, c.opsPartitionKey FROM c WHERE c.id = @id AND c.entityType = @e")
            .WithParameter("@id", orderId)
            .WithParameter("@e", "OpsOrderList");

        var requestOptions = new QueryRequestOptions();
        using var feed = ops.GetItemQueryIterator<OpsPartitionLookup>(sql, requestOptions: requestOptions);

        while (feed.HasMoreResults)
        {
            var page = await feed.ReadNextAsync(cancellationToken);
            foreach (var row in page)
            {
                if (string.IsNullOrEmpty(row.OpsPartitionKey) || row.OpsPartitionKey == newPartitionKey)
                    continue;
                try
                {
                    await ops.DeleteItemAsync<object>(
                        row.Id,
                        new PartitionKey(row.OpsPartitionKey),
                        cancellationToken: cancellationToken);
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // already removed
                }
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
            await _processor.StopAsync();
    }

    private sealed class OpsPartitionLookup
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("opsPartitionKey")]
        public string OpsPartitionKey { get; set; } = "";
    }
}
