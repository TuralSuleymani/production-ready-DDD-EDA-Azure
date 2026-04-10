using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VOEConsulting.Flame.OrderManagement.Infrastructure.Integration;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure;

public static class IntegrationServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IIntegrationEventPublisher"/>: Azure Service Bus when <c>ServiceBus:ConnectionString</c> is set, otherwise <see cref="LoggingIntegrationEventPublisher"/>.
    /// </summary>
    public static IServiceCollection AddIntegrationEventPublishing(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceBusPublisherOptions>(configuration.GetSection(ServiceBusPublisherOptions.SectionName));

        var conn = configuration["ServiceBus:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(conn))
        {
            services.AddSingleton(_ => new ServiceBusClient(conn));
            services.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<ServiceBusClient>();
                var name = sp.GetRequiredService<IOptions<ServiceBusPublisherOptions>>().Value.QueueOrTopicName;
                return client.CreateSender(name);
            });
            services.AddSingleton<IIntegrationEventPublisher, ServiceBusIntegrationEventPublisher>();
        }
        else
        {
            services.AddSingleton<IIntegrationEventPublisher, LoggingIntegrationEventPublisher>();
        }

        return services;
    }

    /// <summary>
    /// Integration audit in Cosmos (<c>integration-log</c>, PK <c>/tenantId</c>). Uses <see cref="NullIntegrationAuditLogger"/> when <c>Cosmos:IntegrationAuditContainer</c> is empty.
    /// </summary>
    public static IServiceCollection AddIntegrationAuditLog(this IServiceCollection services, IConfiguration configuration, string databaseName)
    {
        var containerName = configuration["Cosmos:IntegrationAuditContainer"];
        if (string.IsNullOrWhiteSpace(containerName))
        {
            services.AddSingleton<IIntegrationAuditLogger, NullIntegrationAuditLogger>();
            return services;
        }

        services.AddSingleton<IIntegrationAuditLogger>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            return new CosmosIntegrationAuditLogger(client.GetDatabase(databaseName).GetContainer(containerName));
        });
        return services;
    }
}
