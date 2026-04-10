using System.IO;
using System.Net;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.Azure.Cosmos;
using VOEConsulting.Flame.OrderManagement.Domain.Orders;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

/// <summary>
/// Single-partition transactional batch: replace/create order and create outbox rows with shared <c>tenantId</c> partition key.
/// </summary>
public sealed class CosmosOrderRepository : IOrderRepository
{
    private readonly Container _container;

    public CosmosOrderRepository(Container container)
    {
        _container = container;
    }

    public async Task<OrderPersistenceState?> GetAsync(
        string tenantId,
        string orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<OrderCosmosDocument>(
                orderId,
                new PartitionKey(tenantId),
                cancellationToken: cancellationToken);

            var doc = response.Resource;
            var order = OrderDocumentMapper.ToDomain(doc);
            return new OrderPersistenceState(order, response.ETag);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<UnitResult<DomainError>> SaveAsync(
        string tenantId,
        Order order,
        string? expectedETag,
        IReadOnlyList<OutboxRecord> newOutboxItems,
        CancellationToken cancellationToken = default)
    {
        var pk = new PartitionKey(tenantId);
        var orderDoc = OrderDocumentMapper.ToDocument(tenantId, order);

        var batch = _container.CreateTransactionalBatch(pk);

        if (expectedETag is null)
            batch.CreateItem(orderDoc);
        else
            batch.ReplaceItem(orderDoc.Id, orderDoc, new TransactionalBatchItemRequestOptions { IfMatchEtag = expectedETag });

        foreach (var row in newOutboxItems)
        {
            var outboxDoc = new OutboxCosmosDocument
            {
                Id = row.Id,
                TenantId = row.TenantId,
                EntityType = "Outbox",
                OrderId = row.OrderId,
                EventType = row.EventType,
                PayloadJson = row.PayloadJson,
                Processed = row.Processed,
                Metadata = row.Metadata
            };
            batch.CreateItem(outboxDoc);
        }

        using var response = await batch.ExecuteAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            order.ClearEvents();
            return UnitResult.Success<DomainError>();
        }

        if (response.StatusCode == HttpStatusCode.PreconditionFailed)
            return UnitResult.Failure(DomainError.Conflict("Order was modified by another writer (etag mismatch)."));

        var detail = new StringBuilder()
            .Append("Transactional batch failed: ")
            .Append(response.StatusCode)
            .Append(" — ")
            .AppendLine(response.ErrorMessage ?? "(no batch-level message)");

        for (var i = 0; i < response.Count; i++)
        {
            var op = response[i];
            if (op.IsSuccessStatusCode)
                continue;
            detail.Append("  [").Append(i).Append("] ").Append((int)op.StatusCode).Append(' ').Append(op.StatusCode);
            var body = ReadBatchOpBody(op);
            if (!string.IsNullOrWhiteSpace(body))
                detail.Append(": ").Append(body.Trim());
            detail.AppendLine();
        }

        throw new InvalidOperationException(detail.ToString().TrimEnd());
    }

    private static string? ReadBatchOpBody(TransactionalBatchOperationResult op)
    {
        if (op.ResourceStream is null)
            return null;
        try
        {
            if (op.ResourceStream.CanSeek)
                op.ResourceStream.Position = 0;
            using var reader = new StreamReader(op.ResourceStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            return reader.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }
}
