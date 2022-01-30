using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

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

        public PosProviders? GetProvider(string provider)
        {
            bool supportedProvider = provider.TryParseIgnoreCase(out PosProviders parsedProvider);
            return supportedProvider is false 
                ? null 
                : parsedProvider;
        }

        public ProviderDetails GetProviderDetails(PosProviders provider)
        {
            ProviderSetting providerSetting = _providerSettings.GetSettingByName(provider.ToString());

            string internalMiddlewareName = _getInternalMiddlewareName(provider);
            PublicApiSetting middlewareDestinationSettings = _publicApiSettings.GetSettingByName(internalMiddlewareName);
            string destinationBaseUri = middlewareDestinationSettings.ToBaseUri();
            PublicApiSetting callbackSettings = _publicApiSettings.GetSettingByName("Base");
            string callbackUri = $"{callbackSettings.ToBaseUri()}/auth/callback/{provider}";
            
            string providersRedirectUri = ""; // Delete??

            return new ProviderDetails(
                provider,
                callbackUri,
                destinationBaseUri,
                providersRedirectUri,
                middlewareDestinationSettings,
                providerSetting);
        }
        
        private static string _getInternalMiddlewareName(PosProviders provider)
        {
            return provider switch
            {
                // Step 6: Add map from incoming provider to internal application
                PosProviders.Shopify => PosProviders.Api2Cart.ToString(),
                PosProviders.Squarespace => PosProviders.Api2Cart.ToString(),
                PosProviders.Volusion => PosProviders.Api2Cart.ToString(),
                PosProviders.WoocommerceApi => PosProviders.Api2Cart.ToString(),
                PosProviders.EBay => PosProviders.Api2Cart.ToString(),
                PosProviders.EtsyAPIv3 => PosProviders.Api2Cart.ToString(),
                _ => provider.ToString()
            };
        }
    }

    public class EbayPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
    {
        public EbayPermanentAccessHttpRequestMessage(
            ProviderDetails providerDetails,
            ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
        {
            Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(
                        $"{providerDetails.ProviderSetting.AppId}:{providerDetails.ProviderSetting.AppSecret}")));

            Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                // Potentially write method in Json class to get a property name Json.GetPropertyName(providerDetails.RedirectUri)
                new("redirect_uri", providerDetails.ProviderSetting.AppName!),
                new("grant_type", shortLivedAuthorisationDetail.GrantType),
                new("code", shortLivedAuthorisationDetail.ShortLivedCode)
            });
        }
    }

    public abstract class PermanentAccessHttpRequestMessage : HttpRequestMessage
    {
        protected PermanentAccessHttpRequestMessage(
            ShortLivedAuthorisationDetail shortLivedAuthorisationDetail)
        {
            Method = HttpMethod.Post;
            RequestUri = new Uri(shortLivedAuthorisationDetail.PermanentAccessUrl);
        }
    }

    public class EtsyAPIv3PermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
    {
        public EtsyAPIv3PermanentAccessHttpRequestMessage(
            ProviderDetails providerDetails,
            ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
        {
            Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                // Potentially write method in Json class to get a property name Json.GetPropertyName(providerDetails.RedirectUri)
                new("client_id", providerDetails.ProviderSetting.AppId),
                new("redirect_uri", providerDetails.ProvidersRedirectUri),
                new("grant_type", shortLivedAuthorisationDetail.GrantType),
                new("code", shortLivedAuthorisationDetail.ShortLivedCode),
                new("code_verifier", shortLivedAuthorisationDetail.EncryptedUserInfo),
            });
        }
    }

    public class ShopifyPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
    {
        public ShopifyPermanentAccessHttpRequestMessage(
            ProviderDetails providerDetails,
            ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
        {
            Content = new StringContent(
                Json.Serialize(
                    new ShopifyPermanentAccess(
                        providerDetails.ProviderSetting.AppId,
                        providerDetails.ProviderSetting.AppSecret,
                        shortLivedAuthorisationDetail.ShortLivedCode)),
                Encoding.UTF8,
                Json.MediaType);
        }
    }
}