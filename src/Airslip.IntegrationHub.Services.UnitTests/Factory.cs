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
                        AppId = "SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL",
                        AppSecret = "yujZrOdVKZbGXUvfYP6VjWYluZJ77ge4"
                    }
                },
                {
                    PosProviders.Shopify.ToString(), new ProviderSetting
                    {
                        BaseUri = "https://{0}",
                        AppId = "client-id",
                        AppSecret = "client-secret",
                        Scope = "read_orders,read_products,read_inventory",
                    }
                },
                {
                    PosProviders.Squarespace.ToString(), new ProviderSetting
                    {
                        BaseUri = "https://{0}.squarespace.com",
                        AppId = "client-id",
                        AppSecret = "client-secret",
                        Scope = "website.orders.read,website.transactions.read,website.inventory.read,website.products.read"
                    }
                }
            }
        };

        public static ShortLivedAuthorisationDetail ShortLivedAuthorisationDetail = new()
        {
            ShortLivedCode = "short-lived-code",
            EncryptedUserInfo = "airslip-user-info",
            StoreName = "store-name",
            PermanentAccessUrl = "permanent-access-url",
            BaseUri = "base-uri",
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
            providerSetting.AppId = "app-id";
            providerSetting.AppSecret = "app-secret";
            return providerSetting;
        }
    }
}