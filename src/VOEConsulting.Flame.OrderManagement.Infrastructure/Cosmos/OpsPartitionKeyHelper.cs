namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Cosmos;

/// <summary>Ops read-model partition: <c>{tenantId}|{status}|{yyyy-MM}</c> using UTC month of <paramref name="updatedAt"/>.</summary>
public static class OpsPartitionKeyHelper
{
    public static string Build(string tenantId, string status, DateTimeOffset updatedAt)
    {
        var ym = updatedAt.UtcDateTime.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
        return $"{tenantId}|{status}|{ym}";
    }

    public static string Build(string tenantId, string status, string yearMonth)
        => $"{tenantId}|{status}|{yearMonth}";
}
