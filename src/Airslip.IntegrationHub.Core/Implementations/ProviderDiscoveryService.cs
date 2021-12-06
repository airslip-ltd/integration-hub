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
        private readonly SettingCollection<PublicApiSetting> _publicApiSettings;

        public ProviderDiscoveryService(
            IOptions<SettingCollection<ProviderSetting>> providerOptions, 
            IOptions<SettingCollection<PublicApiSetting>> publicApiOptions)
        {
            _providerSettings = providerOptions.Value;
            _publicApiSettings = publicApiOptions.Value;
        }
        
        public ProviderDetails GetProviderDetails(PosProviders provider)
        {
            ProviderSetting providerSetting = _providerSettings.GetSettingByName(provider.ToString());
            PublicApiSetting publicApiSetting = _publicApiSettings.GetSettingByName(provider.ToString());
            string publicApiBaseUri = publicApiSetting.ToBaseUri();
            
            string callbackUrl =
                $"{publicApiBaseUri}/auth/{provider.ToString().ToLower()}/generate-url";
            
            return new ProviderDetails(
                publicApiBaseUri, 
                providerSetting, 
                callbackUrl);
        }
    }
}