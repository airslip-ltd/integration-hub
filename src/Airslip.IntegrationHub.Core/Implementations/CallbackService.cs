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

    public CallbackService(
        IOptions<EncryptionSettings> encryptionOptions)
    {
        _encryptionSettings = encryptionOptions.Value;
    }

    public IResponse GenerateUrl(ProviderDetails providerDetails, string queryString)
    {
        string callbackUrl = _generateCallbackUrl(providerDetails, queryString);
       
        return new AuthCallbackGeneratorResponse(callbackUrl);
    }

    private string _generateCallbackUrl(
        ProviderDetails providerDetails, 
        string queryString)
    {
        // Move SensitiveCallbackInfo into an interface for testing
        (string cipheredSensitiveInfo, SensitiveCallbackInfo sensitiveCallbackInfo) =
            SensitiveCallbackInfo.GetEncryptedUserInformation(
                queryString,
                _encryptionSettings.PassPhraseToken);
        
        string encodedScope = HttpUtility.UrlEncode(providerDetails.ProviderSetting.Scope);

        // Step 1: Generate link to be used for an OAuth callback
         switch (providerDetails.Provider)
        {
            case PosProviders.EtsyAPIv3:
                return $"{providerDetails.ProviderSetting.BaseUri}/oauth/connect?response_type=code&redirect_uri={providerDetails.CallbackRedirectUri}&scope={encodedScope}&client_id={providerDetails.ProviderSetting.ApiKey}&state={cipheredSensitiveInfo}&code_challenge={cipheredSensitiveInfo}&code_challenge_method=S256";
            case PosProviders.EBay:
                return
                    $"https://auth.sandbox.ebay.com/oauth2/consents?client_id={providerDetails.ProviderSetting.ApiKey}&response_type=code&redirect_uri={providerDetails.CallbackRedirectUri}&scope={encodedScope}&state={cipheredSensitiveInfo}";
            case PosProviders.Vend:
                return
                    $"{providerDetails.ProviderSetting.BaseUri}?response_type=code&client_id={providerDetails.ProviderSetting.ApiKey}&redirect_uri={providerDetails.CallbackRedirectUri}&state={cipheredSensitiveInfo}";
            case PosProviders.SwanRetailMidas:
                return string.Empty;
            case PosProviders.Volusion:
                return string.Empty;
            case PosProviders.Shopify:
                return
                    $"{string.Format(providerDetails.ProviderSetting.FormatBaseUri(sensitiveCallbackInfo.Shop))}/admin/oauth/authorize?client_id={providerDetails.ProviderSetting.ApiKey}&scope={providerDetails.ProviderSetting.Scope}&redirect_uri={providerDetails.CallbackRedirectUri}&state={cipheredSensitiveInfo}&grant_options[]=value";
            case PosProviders.Ecwid:
                return
                    $"https://my.ecwid.com/api/oauth/authorize?client_id={providerDetails.ProviderSetting.ApiKey}&redirect_uri={providerDetails.CallbackRedirectUri.ToLower()}&response_type=code&scope={encodedScope}&state={HttpUtility.UrlDecode(cipheredSensitiveInfo)}"; // Will just go to the app store page. state is for debugging purposes.";
            case PosProviders._3DCart:
                return
                    $"{providerDetails.ProviderSetting.FormatBaseUri("apirest")}/oauth/authorize?client_id={providerDetails.ProviderSetting.ApiKey}&redirect_uri={providerDetails.CallbackRedirectUri}&state={cipheredSensitiveInfo}&response_type=code&store_url=https://{sensitiveCallbackInfo.Shop}.3dcartstores.com";
            case PosProviders.BigcommerceApi:
                return $"https://www.bigcommerce.com/apps/airslip?state={HttpUtility.UrlDecode(cipheredSensitiveInfo)}"; // Will just go to the app store page. state is for debugging purposes.
            case PosProviders.WoocommerceApi:
                return
                    $"{string.Format(providerDetails.ProviderSetting.FormatBaseUri(sensitiveCallbackInfo.Shop))}/wc-auth/v1/authorize?app_name=Airslip&scope={providerDetails.ProviderSetting.Scope}&user_id={cipheredSensitiveInfo}&return_url=https://google.com&callback_url={providerDetails.CallbackRedirectUri}"; // TODO: Change return_url
            case PosProviders.Squarespace:
                return
                    $"https://login.squarespace.com/api/1/login/oauth/provider/authorize?client_id={providerDetails.ProviderSetting.ApiKey}&scope={encodedScope}&redirect_uri={HttpUtility.UrlEncode(providerDetails.CallbackRedirectUri)}&state={cipheredSensitiveInfo}&access_type=offline";
            case PosProviders.AmazonSP:
                string url = $"{providerDetails.ProviderSetting.BaseUri}/apps/authorize/consent?application_id={providerDetails.ProviderSetting.AppName}&state={cipheredSensitiveInfo}&redirect_uri={providerDetails.CallbackRedirectUri}";
                if (providerDetails.ProviderSetting.TestMode == true)
                    url += "&version=beta";
                return url;
            default:
                return string.Empty;
        }
    }
}