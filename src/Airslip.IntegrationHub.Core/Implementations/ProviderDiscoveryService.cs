using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Extensions.Options;

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
}