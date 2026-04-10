using Microsoft.Extensions.Configuration;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Hosting;

/// <summary>Bootstrap settings shared by API and Worker hosts.</summary>
public static class OrderManagementConfiguration
{
    public sealed record AppConfigurationBootstrap(
        string? ConnectionString,
        string? Endpoint,
        string SentinelKey);

    public sealed record CosmosConnectionOptions(
        string ConnectionString,
        string DatabaseName,
        string OrdersContainer,
        string LeaseContainer,
        string OrderSummariesContainer,
        string OrderOpsContainer);

    public static AppConfigurationBootstrap GetAppConfigurationBootstrap(IConfiguration configuration) =>
        new(
            configuration["AppConfig:ConnectionString"],
            configuration["AppConfig:EndpointC"],
            configuration["AppConfig:SentinelKey"] ?? "Settings:Sentinel");

    public static bool IsAppConfigurationEnabled(AppConfigurationBootstrap options) =>
        !string.IsNullOrWhiteSpace(options.ConnectionString)
        || !string.IsNullOrWhiteSpace(options.Endpoint);

    public static CosmosConnectionOptions GetCosmosOptions(IConfiguration configuration)
    {
        var connectionString = configuration["Cosmos:ConnectionString"]
            ?? throw new InvalidOperationException(
                "Set Cosmos:ConnectionString in Azure App Configuration (or User Secrets in Development).");

        return new CosmosConnectionOptions(
            connectionString,
            configuration["Cosmos:DatabaseName"] ?? "orders-db",
            configuration["Cosmos:OrdersContainer"] ?? "orders",
            configuration["Cosmos:LeaseContainer"] ?? "leases",
            configuration["Cosmos:OrderSummariesContainer"] ?? "order-summaries",
            configuration["Cosmos:OrderOpsContainer"] ?? "order-ops-index");
    }
}
