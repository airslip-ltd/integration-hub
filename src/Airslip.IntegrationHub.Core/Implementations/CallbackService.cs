using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using System.Web;

namespace Airslip.IntegrationHub.Core.Implementations;

public class CallbackService : ICallbackService
{
    public IResponse GenerateUrl(ProviderDetails providerDetails, SensitiveCallbackInfo sensitiveCallbackInfo)
    {
        string callbackUrl = _generateCallbackUrl(providerDetails, sensitiveCallbackInfo);
       
        return new AuthorisationResponse(callbackUrl);
    }

    private string _generateCallbackUrl(
        ProviderDetails providerDetails, 
        SensitiveCallbackInfo sensitiveCallbackInfo)
    {
        string encodedScope = HttpUtility.UrlEncode(providerDetails.ProviderSetting.Scope);
        // TODO: Update ebay
        // Step 1: Generate link to be used for an OAuth callback
         switch (providerDetails.Provider)
        {
            case PosProviders.EtsyAPIv3:
                return $"{providerDetails.ProviderSetting.BaseUri}/oauth/connect?response_type=code&redirect_uri={providerDetails.CallbackRedirectUri}&scope={encodedScope}&client_id={providerDetails.ProviderSetting.ApiKey}&state={sensitiveCallbackInfo.CipheredSensitiveInfo}&code_challenge={sensitiveCallbackInfo.CipheredSensitiveInfo}&code_challenge_method=S256";
            case PosProviders.EBay:
                return
                    $"https://auth.sandbox.ebay.com/oauth2/consents?client_id={providerDetails.ProviderSetting.ApiKey}&response_type=code&redirect_uri={providerDetails.ProviderSetting.AppName}&scope={encodedScope}&state={sensitiveCallbackInfo.CipheredSensitiveInfo}";
            case PosProviders.Vend:
                return
                    $"{providerDetails.ProviderSetting.BaseUri}?response_type=code&client_id={providerDetails.ProviderSetting.ApiKey}&redirect_uri={providerDetails.CallbackRedirectUri}&state={sensitiveCallbackInfo.CipheredSensitiveInfo}";
            case PosProviders.SwanRetailMidas:
                return string.Empty;
            case PosProviders.Volusion:
            case PosProviders.Shopify:
                return
                    $"{string.Format(providerDetails.ProviderSetting.FormatBaseUri(sensitiveCallbackInfo.Shop))}/admin/oauth/authorize?client_id={providerDetails.ProviderSetting.ApiKey}&scope={providerDetails.ProviderSetting.Scope}&redirect_uri={providerDetails.CallbackRedirectUri}&state={sensitiveCallbackInfo.CipheredSensitiveInfo}&grant_options[]=value";
            case PosProviders.Ecwid:
                return
                    $"https://my.ecwid.com/api/oauth/authorize?client_id={providerDetails.ProviderSetting.ApiKey}&redirect_uri={providerDetails.CallbackRedirectUri.ToLower()}&response_type=code&scope={encodedScope}&state={HttpUtility.UrlDecode(sensitiveCallbackInfo.CipheredSensitiveInfo)}"; // Will just go to the app store page. state is for debugging purposes.";
            case PosProviders._3DCart:
                string threeDCartUrl = $"{providerDetails.ProviderSetting.FormatBaseUri("apirest")}/oauth/authorize?client_id={providerDetails.ProviderSetting.ApiKey}&redirect_uri={providerDetails.CallbackRedirectUri}&state={sensitiveCallbackInfo.CipheredSensitiveInfo}&response_type=code";
                
                if (!string.IsNullOrWhiteSpace(sensitiveCallbackInfo.Shop))
                    threeDCartUrl += $"&store_url=https://{sensitiveCallbackInfo.Shop}.3dcartstores.com";
                return threeDCartUrl;
            case PosProviders.BigcommerceApi:
                return $"https://www.bigcommerce.com/apps/airslip?state={HttpUtility.UrlDecode(sensitiveCallbackInfo.CipheredSensitiveInfo)}"; // Will just go to the app store page. state is for debugging purposes.
            case PosProviders.WoocommerceApi:
                return $"{string.Format(providerDetails.ProviderSetting.FormatBaseUri(sensitiveCallbackInfo.Shop))}/wc-auth/v1/authorize?app_name=Airslip&scope={providerDetails.ProviderSetting.Scope}&user_id={sensitiveCallbackInfo.CipheredSensitiveInfo}&return_url={providerDetails.ProviderSetting.ReturnPage}&callback_url={providerDetails.CallbackRedirectUri}";
            case PosProviders.Squarespace:
                return
                    $"https://login.squarespace.com/api/1/login/oauth/provider/authorize?client_id={providerDetails.ProviderSetting.ApiKey}&scope={encodedScope}&redirect_uri={HttpUtility.UrlEncode(providerDetails.CallbackRedirectUri)}&state={sensitiveCallbackInfo.CipheredSensitiveInfo}&access_type=offline";
            case PosProviders.AmazonSP:
                string url = $"{providerDetails.ProviderSetting.BaseUri}/apps/authorize/consent?application_id={providerDetails.ProviderSetting.AppName}&state={sensitiveCallbackInfo.CipheredSensitiveInfo}&redirect_uri={providerDetails.CallbackRedirectUri}";
                if (providerDetails.ProviderSetting.Environment == "sandbox")
                    url += "&version=beta";
                return url;
            default:
                return string.Empty;
        }
    }
}