using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;
using System.Net.Http;

namespace Airslip.IntegrationHub.Core.Models.ThreeDCart;

public class ThreeDCartPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public ThreeDCartPermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Method = HttpMethod.Post;
        Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("client_id", providerDetails.ProviderSetting.AppId),
            new("client_secret", providerDetails.ProviderSetting.AppSecret),
            new("grant_type", shortLivedAuthorisationDetail.GrantType),
            new("code", shortLivedAuthorisationDetail.ShortLivedCode),
        });
    }
}