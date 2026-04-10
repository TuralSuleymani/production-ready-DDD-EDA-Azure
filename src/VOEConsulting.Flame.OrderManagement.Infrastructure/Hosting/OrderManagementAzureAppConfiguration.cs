using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VOEConsulting.Flame.OrderManagement.Infrastructure.Hosting;

/// <summary>Optional Azure App Configuration + Key Vault secret resolution for local bootstrap.</summary>
public static class OrderManagementAzureAppConfiguration
{
    /// <summary>
    /// When AppConfig connection string or endpoint is set, adds Azure App Configuration to the configuration pipeline
    /// and registers dynamic refresh. Returns whether the host should call <c>UseAzureAppConfiguration</c> (web apps only).
    /// </summary>
    public static bool TryConfigure(
        ConfigurationManager configuration,
        IHostEnvironment environment,
        IServiceCollection services)
    {
        var bootstrap = OrderManagementConfiguration.GetAppConfigurationBootstrap(configuration);
        if (!OrderManagementConfiguration.IsAppConfigurationEnabled(bootstrap))
            return false;

        configuration.AddAzureAppConfiguration(options =>
        {
            if (!string.IsNullOrWhiteSpace(bootstrap.ConnectionString))
                options.Connect(bootstrap.ConnectionString);
            else
                options.Connect(new Uri(bootstrap.Endpoint!), new DefaultAzureCredential());

            options.ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()));
            options.Select("*", labelFilter: null);
            options.Select("*", environment.EnvironmentName);
            options.ConfigureRefresh(refresh =>
                refresh.Register(bootstrap.SentinelKey, refreshAll: true).SetRefreshInterval(TimeSpan.FromSeconds(30)));
        });

        services.AddAzureAppConfiguration();
        return true;
    }
}
