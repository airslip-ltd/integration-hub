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

        // Step 2: Add provider details to app settings
        public ProviderDetails? GetProviderDetails(string provider, bool? testMode = null)
        {
            bool supportedProvider = provider.TryParseIgnoreCase(out PosProviders parsedProvider);

            if (supportedProvider is false)
                return null;
            
            ProviderSetting providerSetting = _providerSettings.GetSettingByName(provider);

            PublicApiSetting middlewareDestinationSettings = _publicApiSettings.GetSettingByName(providerSetting.MiddlewareDestinationAppName);
            string destinationBaseUri = middlewareDestinationSettings.ToBaseUri();

            string publicApiSettingName = testMode == true ? "Base" : "UI";
            
            PublicApiSetting callbackSettings = _publicApiSettings.GetSettingByName(publicApiSettingName);
            
            string callbackRedirectUri = testMode == true
                ? $"{callbackSettings.ToBaseUri()}/auth/callback/{provider}".ToLower() 
                : $"{callbackSettings.ToBaseUri()}/integrate/complete/hub/{provider}".ToLower();

            return new ProviderDetails(
                parsedProvider,
                callbackRedirectUri,
                destinationBaseUri,
                middlewareDestinationSettings,
                providerSetting);
        }
    }
}