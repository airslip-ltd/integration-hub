using Airslip.IntegrationHub.Core.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Airslip.IntegrationHub.Core.Models.Squarespace;

public class SquarespacePermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public SquarespacePermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Method = HttpMethod.Post;
       
        ProductInfoHeaderValue productValue = new("ScraperBot", "1.0");
        ProductInfoHeaderValue commentValue = new("(+http://www.API.com/ScraperBot.html)");

        Headers.UserAgent.Add(productValue);
        Headers.UserAgent.Add(commentValue);
        
        Headers.Authorization = BuildBasicAuth(
            providerDetails.ProviderSetting.ApiKey,
            providerDetails.ProviderSetting.ApiSecret);
         
        Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("grant_type", shortLivedAuthorisationDetail.GrantType),
            new("code", shortLivedAuthorisationDetail.ShortLivedCode),
            new("redirect_uri", providerDetails.CallbackRedirectUri) // Requires the same redirect_uri used to generate the auth URL
        });
    }
}