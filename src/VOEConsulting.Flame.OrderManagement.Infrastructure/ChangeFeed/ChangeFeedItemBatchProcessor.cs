using Microsoft.Extensions.Logging;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.ChangeFeed;

internal static class ChangeFeedEntityTypes
{
    internal const string Order = "Order";
    internal const string Outbox = "Outbox";
}

/// <summary>
/// Declarative rules for processing one change-feed batch: entity filter, optional skip, validation, and per-item handler.
/// </summary>
internal sealed record ChangeFeedItemHandlerSpec(
    string EntityType,
    Func<ChangeFeedItem, bool> IsValidItem,
    string InvalidItemLogMessage,
    Func<ChangeFeedItem, CancellationToken, Task> HandleItemAsync,
    Func<ChangeFeedItem, bool>? ShouldSkip = null);

internal static class ChangeFeedItemBatchProcessor
{
    public static async Task ProcessAsync(
        IEnumerable<ChangeFeedItem> changes,
        ChangeFeedItemHandlerSpec spec,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(changes);
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(logger);

        foreach (var item in changes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.Equals(item.EntityType, spec.EntityType, StringComparison.Ordinal))
                continue;

            if (spec.ShouldSkip?.Invoke(item) == true)
                continue;

            if (!spec.IsValidItem(item))
            {
                logger.LogWarning(spec.InvalidItemLogMessage);
                continue;
            }

            await spec.HandleItemAsync(item, cancellationToken).ConfigureAwait(false);
        }
    }
}
