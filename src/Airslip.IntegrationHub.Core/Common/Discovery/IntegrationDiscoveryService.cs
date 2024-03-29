using Airslip.Common.Types.Configuration;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Enums;
using Microsoft.Extensions.Options;
using Serilog;
using System;

namespace Airslip.IntegrationHub.Core.Common.Discovery;

public class IntegrationDiscoveryService : IIntegrationDiscoveryService
{
    private readonly PublicApiSettings _settings;
    private readonly SettingCollection<IntegrationSetting> _integrationSettings;
    private readonly ILogger _logger;

    public IntegrationDiscoveryService(
        IOptions<SettingCollection<IntegrationSetting>> integrationSettings,
        IOptions<PublicApiSettings> options, ILogger logger)
    {
        _logger = logger;
        _settings = options.Value;
        _integrationSettings = integrationSettings.Value;
    }

    public IntegrationDetails GetIntegrationDetails(string provider, string? integration = null, bool testMode = false)
    {
        IntegrationSetting integrationSetting =  _integrationSettings.GetSettingByName(provider);
        
        if (integrationSetting == null || integrationSetting.PublicApiSettingName == string.Empty)
            return new IntegrationNotFound();

        PublicApiSetting integrationDestinationSetting =
                    _settings.GetSettingByName(integrationSetting.PublicApiSettingName);

        integrationSetting.PublicApiSetting = integrationDestinationSetting;

        string uri = string.Empty;
        string apiKey = string.Empty;
        string callbackUrl = string.Empty;
        switch (integrationSetting.AuthorisationRouteType)
        {
            case AuthorisationRouteType.Internal:
                
                PublicApiSetting baseSetting = _settings.GetSettingByName("Base");
                uri = integrationDestinationSetting.ToBaseUri();
                apiKey = integrationDestinationSetting.ApiKey;
                callbackUrl = $"{baseSetting.ToBaseUri()}/auth/callback/{provider.ToLower()}";
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