using Airslip.Common.Security.Implementations;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Implementations
{
    public class ProviderDiscoveryService : IProviderDiscoveryService
    {
        private readonly SettingCollection<ProviderSetting> _providerSettings;
        private readonly PublicApiSettings _publicApiSettings;

        public ProviderDiscoveryService(
            IOptions<SettingCollection<ProviderSetting>> providerOptions, 
            IOptions<PublicApiSettings> publicApiOptions)
        {
            _providerSettings = providerOptions.Value;
            _publicApiSettings = publicApiOptions.Value;
        }
        
        public ProviderDetails GetProviderDetails(string provider)
        {
            ProviderSetting providerSetting = _providerSettings.GetSettingByName(provider);
            string internalProviderName = GetInternalProviderName(provider);
            PublicApiSetting publicApiSetting = _publicApiSettings.GetSettingByName(internalProviderName);
            string destinationBaseUri = publicApiSetting.ToBaseUri();
            
            string callbackUrl = $"{destinationBaseUri}/auth/{provider.ToLower()}/callback";
            
            return new ProviderDetails(
                destinationBaseUri,
                publicApiSetting,
                providerSetting, 
                callbackUrl);
        }
        
        public string GenerateCallbackUrl(PosProviders provider, string accountId, string? shopName = null, bool? isOnline = false, string? redirectUri = null)
        {
            ProviderSetting providerSetting = _providerSettings.GetSettingByName(provider.ToString());
            redirectUri ??= providerSetting.RedirectUri;
            
            switch (provider)
            {
                case PosProviders.Vend:
                    return
                        $"{providerSetting.BaseUri}?response_type=code&client_id={providerSetting.ClientId}&redirect_uri={redirectUri}&state={accountId}";
                case PosProviders.SwanRetailMidas:
                case PosProviders.Volusion:
                    return string.Empty;
                case PosProviders.Shopify:
                    string grantOptions = isOnline == true ? "per-user" : "value";
                    return $"{string.Format(providerSetting.BaseUri, shopName)}/admin/oauth/authorize?client_id={providerSetting.ClientId}&scope=read_orders,read_products,read_inventory&redirect_uri={redirectUri}&state={accountId}&grant_options[]={grantOptions}";
                case PosProviders.Stripe:
                case PosProviders.SumUp:
                case PosProviders.IZettle:
                case PosProviders.EposNow:
                case PosProviders.Square:
                    return string.Empty;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, "Not yet supported");
            }
        }

        public bool Validate(PosProviders provider, List<KeyValuePair<string, string>> queryStrings, string hmacKey)
        {
            KeyValuePair<string, string> hmacKeyValuePair = queryStrings.Get(hmacKey);
            string hmacValue = hmacKeyValuePair.Value;
            queryStrings.Remove(hmacKeyValuePair);
            ProviderSetting providerSetting = _providerSettings.GetSettingByName(provider.ToString());

            return HmacCipher.Validate(queryStrings, hmacValue, providerSetting.ClientSecret);
        }

        private static string GetInternalProviderName(string provider)
        {
            PosProviders posProvider = Enum.Parse<PosProviders>(provider);
            return posProvider switch
            {
                PosProviders.Shopify => PosProviders.Api2Cart.ToString(),
                PosProviders.Volusion => PosProviders.Api2Cart.ToString(),
                _ => provider
            };
        }
    }
}