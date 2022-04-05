using Airslip.Common.Types.Configuration;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Enums;
using Microsoft.Extensions.Options;

namespace Airslip.IntegrationHub.Core.Common.Discovery;

public class IntegrationDiscoveryService : IIntegrationDiscoveryService
{
    private readonly PublicApiSettings _settings;
    private readonly SettingCollection<IntegrationSetting> _integrationSettings;

    public IntegrationDiscoveryService(
        IOptions<SettingCollection<IntegrationSetting>> integrationSettings, 
        IOptions<PublicApiSettings> options)
    {
        _settings = options.Value;
        _integrationSettings = integrationSettings.Value;
    }
        
    public IntegrationDetails GetIntegrationDetails(string provider, string integration, bool testMode = false)
    {
        IntegrationSetting integrationSetting = _integrationSettings.GetSettingByName(provider);
        string uri = string.Empty;
        string apiKey = string.Empty;
        string callbackUrl = string.Empty;
        switch (integrationSetting.AuthorisationRouteType)
        {
            case AuthorisationRouteType.Internal: 
                PublicApiSetting setting = _settings.GetSettingByName(integrationSetting.PublicApiSettingName);
                uri = setting.ToBaseUri();
                apiKey = setting.ApiKey;
                callbackUrl = integrationSetting.SourceType switch
                {
                    SourceType.SingleSource => $"{_settings.Base.ToBaseUri()}/providers/{provider.ToLower()}/authorised",
                    _ => $"{_settings.Base.ToBaseUri()}/providers/{provider.ToLower()}/{integration.ToLower()}/authorised"
                };
                break;
            case AuthorisationRouteType.External:
                uri = integrationSetting.AuthorisationBaseUri;
                
                string publicApiSettingName = testMode ? "Base" : "UI";
            
                PublicApiSetting callbackSettings = _settings.GetSettingByName(publicApiSettingName);
            
                callbackUrl = testMode
                    ? $"{callbackSettings.ToBaseUri()}/auth/callback/{provider}".ToLower() 
                    : $"{callbackSettings.ToBaseUri()}/integrate/complete/hub/{provider}".ToLower();
                break;
            default:
                break;
        }

        return new IntegrationDetails(
            uri, 
            apiKey, 
            integrationSetting,
            callbackUrl);
    }
}