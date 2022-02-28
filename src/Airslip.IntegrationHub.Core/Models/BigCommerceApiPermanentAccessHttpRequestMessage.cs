using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;
using System.Net.Http;

namespace Airslip.IntegrationHub.Core.Models;

public class BigCommerceApiPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public BigCommerceApiPermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Method = HttpMethod.Post;
        Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            // Potentially write method in Json class to get a property name Json.GetPropertyName(providerDetails.RedirectUri)
            new("client_id", providerDetails.ProviderSetting.ApiKey),
            new("client_secret", providerDetails.ProviderSetting.ApiSecret),
            new("code", shortLivedAuthorisationDetail.ShortLivedCode),
            new("scope", providerDetails.ProviderSetting.Scope),
            new("grant_type", shortLivedAuthorisationDetail.GrantType),
            new("redirect_uri", providerDetails.CallbackRedirectUri),
            new("context", shortLivedAuthorisationDetail.MiscellaneousInfo),
        });
    }
}