using Microsoft.Azure.Cosmos;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

public sealed class CosmosOpsOrderQuery : IOpsOrderQuery
{
    private readonly Container _ops;

    public CosmosOpsOrderQuery(Container opsContainer)
    {
        _ops = opsContainer;
    }

    public async Task<IReadOnlyList<OpsOrderDto>> ListByStatusMonthAsync(
        string tenantId,
        string status,
        string yearMonth,
        CancellationToken cancellationToken = default)
    {
        var pkValue = OpsPartitionKeyHelper.Build(tenantId, status, yearMonth);
        var query = new QueryDefinition("SELECT * FROM c WHERE c.entityType = @e")
            .WithParameter("@e", "OpsOrderList");

        var options = new QueryRequestOptions { PartitionKey = new PartitionKey(pkValue) };
        var results = new List<OpsOrderDto>();
        using var feed = _ops.GetItemQueryIterator<OpsOrderListCosmosDocument>(query, requestOptions: options);

        while (feed.HasMoreResults)
        {
            var page = await feed.ReadNextAsync(cancellationToken);
            foreach (var doc in page)
                results.Add(Map(doc));
        }

        return results;
    }

    private static OpsOrderDto Map(OpsOrderListCosmosDocument doc) =>
        new(
            doc.Id,
            doc.TenantId,
            doc.CustomerId,
            doc.Status,
            doc.YearMonth,
            doc.OpsPartitionKey,
            doc.PlacedAt,
            doc.UpdatedAt);
}
