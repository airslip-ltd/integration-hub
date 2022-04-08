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
        
    public IntegrationDetails GetIntegrationDetails(string provider, string? integration = null, bool testMode = false)
    {
        IntegrationSetting integrationSetting = _integrationSettings.GetSettingByName(provider);

        string uri = string.Empty;
        string apiKey = string.Empty;
        string callbackUrl = string.Empty;
        switch (integrationSetting.AuthorisationRouteType)
        {
            case AuthorisationRouteType.Internal: 
                PublicApiSetting integrationDestinationSetting = _settings.GetSettingByName(integrationSetting.PublicApiSettingName);
                PublicApiSetting baseSetting = _settings.GetSettingByName("Base");
                uri = integrationDestinationSetting.ToBaseUri();
                apiKey = integrationDestinationSetting.ApiKey;
                callbackUrl = integrationSetting.SourceType switch
                {
                    SourceType.SingleSource => $"{baseSetting.ToBaseUri()}/auth/callback/{provider.ToLower()}",
                    _ => $"{baseSetting.ToBaseUri()}/auth/callback/{provider.ToLower()}/{integration?.ToLower()}" // Do we need this line?
                };
                break;
            case AuthorisationRouteType.External:
                uri = integrationSetting.AuthorisationBaseUri;
                
                string publicApiSettingName = testMode ? "Base" : "UI";
            
                PublicApiSetting callbackSettings = _settings.GetSettingByName(publicApiSettingName);

                callbackUrl = testMode
                    ? $"{callbackSettings.ToBaseUri()}/auth/callback/{provider}".ToLower() 
                    : $"{callbackSettings.ToBaseUri()}/integrate/complete/hub/{provider}".ToLower();
                
                if (!string.IsNullOrEmpty(integrationSetting.ReturnPageFormat))
                    _formatReturnPage(testMode, integrationSetting, callbackSettings);
             
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

    private void _formatReturnPage(
        bool testMode, IntegrationSetting integrationSetting, PublicApiSetting callbackSettings)
    {
        if (testMode)
            callbackSettings = _settings.GetSettingByName("UI");

        integrationSetting.FormatReturnPage(callbackSettings.BaseUri);
    }
}