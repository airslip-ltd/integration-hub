using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;
using System.Net.Http;

namespace Airslip.IntegrationHub.Core.Models.eBay;

public class EbayPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public EbayPermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Method = HttpMethod.Post;
        Headers.Authorization = BuildBasicAuth(
            providerDetails.ProviderSetting.ApiKey, 
            providerDetails.ProviderSetting.ApiSecret);

        Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            // Potentially write method in Json class to get a property name Json.GetPropertyName(providerDetails.RedirectUri)
            new("redirect_uri", providerDetails.ProviderSetting.AppName!),
            new("grant_type", shortLivedAuthorisationDetail.GrantType),
            new("code", shortLivedAuthorisationDetail.ShortLivedCode)
        });
    }
}