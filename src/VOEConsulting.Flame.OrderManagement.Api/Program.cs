var builder = WebApplication.CreateBuilder(args);

var useAzureAppConfig = OrderManagementAzureAppConfiguration.TryConfigure(
    builder.Configuration,
    builder.Environment,
    builder.Services);

var cosmos = OrderManagementConfiguration.GetCosmosOptions(builder.Configuration);

builder.Services.AddOrderManagementPersistence(builder.Configuration, cosmos);
builder.Services.AddOrderSummaryQueries(cosmos.DatabaseName, cosmos.OrderSummariesContainer);
builder.Services.AddOrderOpsQueries(cosmos.DatabaseName, cosmos.OrderOpsContainer);
builder.Services.AddOrderManagementApplication();

var app = builder.Build();

if (useAzureAppConfig)
    app.UseAzureAppConfiguration();

app.MapOrderManagementEndpoints();
app.Run();
