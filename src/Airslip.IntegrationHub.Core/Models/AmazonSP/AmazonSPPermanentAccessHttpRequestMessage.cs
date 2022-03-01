using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;
using System.Net.Http;

namespace Airslip.IntegrationHub.Core.Models.AmazonSP;

public class AmazonSPPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public AmazonSPPermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Method = HttpMethod.Post;
        Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("client_id", providerDetails.ProviderSetting.ApiKey),
            new("client_secret", providerDetails.ProviderSetting.ApiSecret),
            new("grant_type", shortLivedAuthorisationDetail.GrantType),
            new("code", shortLivedAuthorisationDetail.ShortLivedCode),
            new("redirect_uri", providerDetails.CallbackRedirectUri.ToLower())
        });
    }
}