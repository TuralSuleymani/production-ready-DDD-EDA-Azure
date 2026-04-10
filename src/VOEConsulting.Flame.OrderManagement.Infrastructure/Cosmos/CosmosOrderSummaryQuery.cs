using System.Net;
using Microsoft.Azure.Cosmos;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

public sealed class CosmosOrderSummaryQuery : IOrderSummaryQuery
{
    private readonly Container _summaries;

    public CosmosOrderSummaryQuery(Container summariesContainer)
    {
        _summaries = summariesContainer;
    }

    public async Task<OrderSummaryDto?> GetAsync(
        string customerId,
        string orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _summaries
                .ReadItemAsync<OrderSummaryCosmosDocument>(
                    orderId,
                    new PartitionKey(customerId),
                    cancellationToken: cancellationToken)
                ;
            return Map(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> ListByCustomerAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM c WHERE c.entityType = @t";
        var query = new QueryDefinition(sql).WithParameter("@t", "OrderSummary");
        var options = new QueryRequestOptions { PartitionKey = new PartitionKey(customerId) };

        var results = new List<OrderSummaryDto>();
        using var feed = _summaries.GetItemQueryIterator<OrderSummaryCosmosDocument>(query, requestOptions: options);

        while (feed.HasMoreResults)
        {
            var page = await feed.ReadNextAsync(cancellationToken);
            foreach (var doc in page)
                results.Add(Map(doc));
        }

        return results;
    }

    private static OrderSummaryDto Map(OrderSummaryCosmosDocument doc)
    {
        var placed = doc.PlacedAt ?? doc.UpdatedAt;
        return new OrderSummaryDto(doc.Id, doc.CustomerId, doc.TenantId, doc.Status, doc.UpdatedAt, placed);
    }
}
