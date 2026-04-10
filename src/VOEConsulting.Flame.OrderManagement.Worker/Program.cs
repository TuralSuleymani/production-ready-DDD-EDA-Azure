using VOEConsulting.Flame.OrderManagement.Infrastructure.Hosting;

var builder = Host.CreateApplicationBuilder(args);

_ = OrderManagementAzureAppConfiguration.TryConfigure(
    builder.Configuration,
    builder.Environment,
    builder.Services);

var cosmos = OrderManagementConfiguration.GetCosmosOptions(builder.Configuration);

builder.Services.AddOrderManagementPersistence(builder.Configuration, cosmos);
builder.Services.AddOrderManagementChangeFeedProjectors(builder.Configuration, cosmos);

var host = builder.Build();
await host.RunAsync();
