using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;
using System.Net.Http;

namespace Airslip.IntegrationHub.Core.Models;

public class EtsyAPIv3PermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public EtsyAPIv3PermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            // Potentially write method in Json class to get a property name Json.GetPropertyName(providerDetails.RedirectUri)
            new("client_id", providerDetails.ProviderSetting.AppId),
            new("redirect_uri", providerDetails.CallbackRedirectUri),
            new("grant_type", shortLivedAuthorisationDetail.GrantType),
            new("code", shortLivedAuthorisationDetail.ShortLivedCode),
            new("code_verifier", shortLivedAuthorisationDetail.EncryptedUserInfo),
        });
    }
}