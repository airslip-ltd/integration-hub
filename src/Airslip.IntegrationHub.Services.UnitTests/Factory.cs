using Airslip.Common.Testing;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Services.UnitTests
{
    public static class Factory
    {
        public static string _projectName = "Airslip.IntegrationHub";

        //TODO: Try and get from appsettings.json
        public static SettingCollection<ProviderSetting> ProviderConfiguration = new()
        {
            Settings = new Dictionary<string, ProviderSetting>
            {
                {
                    PosProviders.Vend.ToString(), new ProviderSetting
                    {
                        BaseUri = "https://secure.vendhq.com/connect",
                        ApiKey = "SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL",
                        ApiSecret = "yujZrOdVKZbGXUvfYP6VjWYluZJ77ge4"
                    }
                },
                {
                    PosProviders.Shopify.ToString(), new ProviderSetting
                    {
                        BaseUri = "https://{0}",
                        ApiKey = "client-id",
                        ApiSecret = "client-secret",
                        Scope = "read_orders,read_products,read_inventory",
                    }
                },
                {
                    PosProviders.Squarespace.ToString(), new ProviderSetting
                    {
                        BaseUri = "https://{0}.squarespace.com",
                        ApiKey = "client-id",
                        ApiSecret = "client-secret",
                        Scope = "website.orders.read,website.transactions.read,website.inventory.read,website.products.read"
                    }
                }
            }
        };

        public static PublicApiSetting GetPublicApiSetting(PosProviders posProvider)
        {
            IConfiguration appSettingsConfig = OptionsMock.InitialiseConfiguration(_projectName)!;

            SettingCollection<ProviderSetting> providerSettings = new();
            appSettingsConfig.GetSection("ProviderSettings").Bind(providerSettings);
            ProviderSetting providerSetting = providerSettings.GetSettingByName(posProvider.ToString());
            
            Mock<IOptions<PublicApiSettings>> publicApiSettingsMock = OptionsMock.SetUpOptionSettings<PublicApiSettings>(_projectName)!;
            return publicApiSettingsMock.Object.Value.GetSettingByName(providerSetting.MiddlewareDestinationAppName);
        }
        
        public static ProviderSetting GetProviderSetting(PosProviders posProvider)
        {
            IConfiguration appSettingsConfig = OptionsMock.InitialiseConfiguration(_projectName)!;

            SettingCollection<ProviderSetting> providerSettings = new();
            appSettingsConfig.GetSection("ProviderSettings").Bind(providerSettings);
            ProviderSetting providerSetting = providerSettings.GetSettingByName(posProvider.ToString());
            providerSetting.ApiKey = "app-id";
            providerSetting.ApiSecret = "app-secret";
            return providerSetting;
        }
    }
}