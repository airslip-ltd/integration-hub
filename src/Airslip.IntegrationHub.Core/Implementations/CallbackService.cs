using Airslip.Common.Security.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Responses;
using Microsoft.Extensions.Options;
using System.Web;

namespace Airslip.IntegrationHub.Core.Implementations;

public class CallbackService : ICallbackService
{
    private readonly EncryptionSettings _encryptionSettings;
    private readonly IProviderDiscoveryService _providerDiscoveryService;

    public CallbackService(
        IOptions<EncryptionSettings> encryptionOptions, 
        IProviderDiscoveryService providerDiscoveryService)
    {
        _encryptionSettings = encryptionOptions.Value;
        _providerDiscoveryService = providerDiscoveryService;
    }

    public IResponse GenerateUrl(string provider, string queryString)
    {
       bool supportedProvider =  provider.TryParseIgnoreCase(out PosProviders parsedProvider);
        
        if (supportedProvider)
            return new ErrorResponse("PARSE_ERROR", $"{provider} is an unsupported provider");
        
        ProviderDetails providerDetails = _providerDiscoveryService.GetProviderDetails(parsedProvider);

        string callbackUrl = _generateCallbackUrl(providerDetails, queryString);
       
        return new AuthCallbackGeneratorResponse(callbackUrl);
    }

    private string _generateCallbackUrl(
        ProviderDetails providerDetails, 
        string queryString)
    {
        (string cipheredSensitiveInfo, SensitiveCallbackInfo sensitiveCallbackInfo) =
            SensitiveCallbackInfo.GetEncryptedUserInformation(
                queryString,
                _encryptionSettings.PassPhraseToken);
        
        string redirectUri = string.IsNullOrEmpty(sensitiveCallbackInfo.CallbackUrl)
            ? providerDetails.CallbackRedirectUri
            : sensitiveCallbackInfo.CallbackUrl;
        
        string encodedScope = HttpUtility.UrlEncode(providerDetails.ProviderSetting.Scope);

        // Step 1: Generate link to be used for an OAuth callback
         switch (providerDetails.Provider)
        {
            case PosProviders.EtsyAPIv3:
                return $"{providerDetails.ProviderSetting.BaseUri}/oauth/connect?response_type=code&redirect_uri={redirectUri}&scope={encodedScope}&client_id={providerDetails.ProviderSetting.AppId}&state={cipheredSensitiveInfo}&code_challenge={cipheredSensitiveInfo}&code_challenge_method=S256";
            case PosProviders.EBay:
                return
                    $"https://auth.sandbox.ebay.com/oauth2/consents?client_id={providerDetails.ProviderSetting.AppId}&response_type=code&redirect_uri={redirectUri}&scope={encodedScope}&state={cipheredSensitiveInfo}";
            case PosProviders.Vend:
                return
                    $"{providerDetails.ProviderSetting.BaseUri}?response_type=code&client_id={providerDetails.ProviderSetting.AppId}&redirect_uri={redirectUri}&state={cipheredSensitiveInfo}";
            case PosProviders.SwanRetailMidas:
            case PosProviders.Volusion:
            case PosProviders.Shopify:
                ShopifyProvider auth = queryString.GetQueryParams<ShopifyProvider>();
                string grantOptions = auth.IsOnline ? "per-user" : "value";
                return
                    $"{string.Format(providerDetails.ProviderSetting.FormatBaseUri(sensitiveCallbackInfo.Shop))}/admin/oauth/authorize?client_id={providerDetails.ProviderSetting.AppId}&scope={providerDetails.ProviderSetting.Scope}&redirect_uri={redirectUri}&state={cipheredSensitiveInfo}&grant_options[]={grantOptions}";
            case PosProviders.Stripe:
            case PosProviders.SumUp:
            case PosProviders.IZettle:
            case PosProviders.BigcommerceApi:
            case PosProviders.WoocommerceApi:
                return
                    $"{string.Format(providerDetails.ProviderSetting.FormatBaseUri(sensitiveCallbackInfo.Shop))}/wc-auth/v1/authorize?app_name=Airslip&scope={providerDetails.ProviderSetting.Scope}&user_id={cipheredSensitiveInfo}&return_url=https://google.com&callback_url={redirectUri}";
            case PosProviders.Squarespace:
                return
                    $"{string.Format(providerDetails.ProviderSetting.FormatBaseUri(sensitiveCallbackInfo.Shop))}/api/1/login/oauth/provider/authorize?client_id={providerDetails.ProviderSetting.AppId}&scope={providerDetails.ProviderSetting.Scope}&redirect_uri={redirectUri}&state={cipheredSensitiveInfo}&access_type=offline";
            default:
                return string.Empty;
        }
    }
}