using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.IntegrationHub.Core.Models;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Services.UnitTests
{
    public static class Factory
    {
        public static SettingCollection<ProviderSetting> ProviderConfiguration = new()
        {
            Settings = new Dictionary<string, ProviderSetting>
            {
                {
                    PosProviders.Vend.ToString(), new ProviderSetting
                    {
                        BaseUri = "https://secure.vendhq.com/connect",
                        ClientId = "SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL",
                        ClientSecret = "yujZrOdVKZbGXUvfYP6VjWYluZJ77ge4",
                        RedirectUri = "http://localhost:38101/v1/auth"
                    }
                },
                {
                    PosProviders.Shopify.ToString(), new ProviderSetting
                    {
                        BaseUri = "https://{0}.myshopify.com",
                        ClientId = "client-id",
                        ClientSecret = "client-secret",
                        RedirectUri = "http://localhost:31201/v1/auth/callback"
                    }
                }
            }
        };
    }
}