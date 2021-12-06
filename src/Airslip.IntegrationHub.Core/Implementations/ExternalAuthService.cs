using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using Microsoft.Extensions.Options;
using System;

namespace Airslip.IntegrationHub.Core.Implementations
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly SettingCollection<ProviderSetting> _providerSettings;
        public ExternalAuthService(
            IOptions<SettingCollection<ProviderSetting>> providerOptions)
        {
            _providerSettings = providerOptions.Value;
        }

        public string GenerateCallbackUrl(PosProviders provider, string accountId, string? redirectUri = null)
        {
            ProviderSetting providerSetting = _providerSettings.GetSettingByName(provider.ToString());
            
            redirectUri ??= providerSetting.RedirectUri;

            return provider switch
            {
                PosProviders.Vend =>
                    $"{providerSetting.BaseUri}?response_type=code&client_id={providerSetting.ClientId}&redirect_uri={redirectUri}&state={accountId}",
                PosProviders.SwanRetailMidas => string.Empty,
                PosProviders.Api2CartVolusion => string.Empty,
                PosProviders.Api2CartShopify => string.Empty,
                PosProviders.Stripe => string.Empty,
                PosProviders.SumUp => string.Empty,
                PosProviders.IZettle => string.Empty,
                PosProviders.EposNow => string.Empty,
                PosProviders.Square => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Not yet supported")
            };
        }
        
        public AuthorisationCallBackBase GetQueryParams(PosProviders provider, string queryStrings)
        {
            return provider switch
            {
                PosProviders.Vend => queryStrings.GetQueryParams<VendAuthorisationCallBackRequest>(),
                PosProviders.SwanRetailMidas => new AuthorisationCallBackBase(),
                PosProviders.Api2CartVolusion => new AuthorisationCallBackBase(),
                PosProviders.Api2CartShopify => new AuthorisationCallBackBase(),
                PosProviders.Stripe => new AuthorisationCallBackBase(),
                PosProviders.SumUp => new AuthorisationCallBackBase(),
                PosProviders.IZettle => new AuthorisationCallBackBase(),
                PosProviders.EposNow => new AuthorisationCallBackBase(),
                PosProviders.Square => new AuthorisationCallBackBase(),
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
            };
        }
    }
}