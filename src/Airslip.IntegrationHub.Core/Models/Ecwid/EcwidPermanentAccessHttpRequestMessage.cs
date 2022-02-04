using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;
using System.Net.Http;

namespace Airslip.IntegrationHub.Core.Models.Ecwid;

public class EcwidPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public EcwidPermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Method = HttpMethod.Get;
        Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("client_id", providerDetails.ProviderSetting.AppId),
            new("client_secret", providerDetails.ProviderSetting.AppSecret),
            new("grant_type", shortLivedAuthorisationDetail.GrantType),
            new("code", shortLivedAuthorisationDetail.ShortLivedCode),
            new("redirect_uri", providerDetails.CallbackRedirectUri.ToLower())
        });
    }
}