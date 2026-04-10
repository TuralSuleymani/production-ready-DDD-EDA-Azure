using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

public sealed class OutboxChangeFeedHostedService : IHostedService
{
    private readonly CosmosClient _cosmosClient;
    private readonly OutboxChangeFeedOptions _options;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly IOutboxProcessedMarker _outboxProcessedMarker;
    private readonly IIntegrationAuditLogger _audit;
    private readonly ILogger<OutboxChangeFeedHostedService> _logger;
    private ChangeFeedProcessor? _processor;

    public OutboxChangeFeedHostedService(
        CosmosClient cosmosClient,
        IOptions<OutboxChangeFeedOptions> options,
        IIntegrationEventPublisher publisher,
        IOutboxProcessedMarker outboxProcessedMarker,
        IIntegrationAuditLogger audit,
        ILogger<OutboxChangeFeedHostedService> logger)
    {
        _cosmosClient = cosmosClient;
        _options = options.Value;
        _publisher = publisher;
        _outboxProcessedMarker = outboxProcessedMarker;
        _audit = audit;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ThrowIfOptionsInvalid();

        var database = ChangeFeedProcessorBootstrap.EnsureDatabase(_cosmosClient, _options.DatabaseName);
        var monitored = ChangeFeedProcessorBootstrap.EnsureOrdersMonitoredContainer(database, _options.OrdersContainerName);
        var lease = ChangeFeedProcessorBootstrap.EnsureLeaseContainer(database, _options.LeaseContainerName);

        _processor = monitored
            .GetChangeFeedProcessorBuilder<ChangeFeedItem>(_options.ProcessorName, HandleChangesAsync)
            .WithInstanceName(_options.InstanceName)
            .WithLeaseContainer(lease)
            .Build();

        return _processor.StartAsync();
    }

    private void ThrowIfOptionsInvalid()
    {
        if (string.IsNullOrWhiteSpace(_options.DatabaseName)
            || string.IsNullOrWhiteSpace(_options.OrdersContainerName)
            || string.IsNullOrWhiteSpace(_options.LeaseContainerName))
        {
            throw new InvalidOperationException(
                "OutboxChangeFeedOptions.DatabaseName, OrdersContainerName, and LeaseContainerName must be set.");
        }
    }

    private Task HandleChangesAsync(
        ChangeFeedProcessorContext context,
        IReadOnlyCollection<ChangeFeedItem> changes,
        CancellationToken cancellationToken)
    {
        var spec = new ChangeFeedItemHandlerSpec(
            EntityType: ChangeFeedEntityTypes.Outbox,
            IsValidItem: item => !string.IsNullOrEmpty(item.TenantId) && !string.IsNullOrEmpty(item.Id),
            InvalidItemLogMessage: "Skipping outbox item with missing tenantId or id.",
            HandleItemAsync: DispatchOutboxItemAsync,
            ShouldSkip: item => item.Processed == true);

        return ChangeFeedItemBatchProcessor.ProcessAsync(changes, spec, _logger, cancellationToken);
    }

    private async Task DispatchOutboxItemAsync(ChangeFeedItem item, CancellationToken cancellationToken)
    {
        var orderId = item.OrderId ?? "";
        var eventType = item.EventType ?? "";
        var payload = item.PayloadJson ?? "";

        try
        {
            await _publisher.PublishOutboxAsync(item.TenantId, item.Id, orderId, eventType, payload, cancellationToken)
                .ConfigureAwait(false);
            await _audit.LogPublishAsync(item.TenantId, orderId, item.Id, eventType, payload, cancellationToken)
                .ConfigureAwait(false);
            await _outboxProcessedMarker.MarkProcessedAsync(item.TenantId, item.Id, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish/mark outbox {OutboxId} for tenant {TenantId}; lease will retry.",
                item.Id,
                item.TenantId);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
            await _processor.StopAsync();
    }
}
