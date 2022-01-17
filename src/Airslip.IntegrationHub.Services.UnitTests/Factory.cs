using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.IntegrationHub.Core.Models;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Services.UnitTests
{
    public static class Factory
    {
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
                        AppSecret = "client-secret"
                    }
                }
            }
        };

        public static ProviderAuthorisingDetail ProviderAuthorisingDetail = new()
        {
            ShortLivedCode = "short-lived-code",
            EncryptedUserInfo = "airslip-user-info",
            StoreName = "store-name",
            PermanentAccessUrl = "permanent-access-url",
            BaseUri = "base-uri",
        };

    }
}