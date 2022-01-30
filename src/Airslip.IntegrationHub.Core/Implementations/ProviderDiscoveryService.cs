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
        public ProviderDetails GetProviderDetails(PosProviders provider)
        {
            ProviderSetting providerSetting = _providerSettings.GetSettingByName(provider.ToString());

            PublicApiSetting middlewareDestinationSettings = _publicApiSettings.GetSettingByName(providerSetting.MiddlewareDestinationAppName);
            string destinationBaseUri = middlewareDestinationSettings.ToBaseUri();
            PublicApiSetting callbackSettings = _publicApiSettings.GetSettingByName("Base");
            string callbackRedirectUri = $"{callbackSettings.ToBaseUri()}/auth/callback/{provider}";
            
            return new ProviderDetails(
                provider,
                callbackRedirectUri,
                destinationBaseUri,
                middlewareDestinationSettings,
                providerSetting);
        }
    }
}