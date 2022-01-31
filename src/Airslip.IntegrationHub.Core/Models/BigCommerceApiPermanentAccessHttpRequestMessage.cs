using Airslip.IntegrationHub.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Airslip.IntegrationHub.Core.Models;

public class BigCommerceApiPermanentAccessHttpRequestMessage : PermanentAccessHttpRequestMessage
{
    public BigCommerceApiPermanentAccessHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail) : base(shortLivedAuthorisationDetail)
    {
        Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            // Potentially write method in Json class to get a property name Json.GetPropertyName(providerDetails.RedirectUri)
            new("client_id", providerDetails.ProviderSetting.AppId),
            new("client_secret", providerDetails.ProviderSetting.AppSecret),
            new("code", shortLivedAuthorisationDetail.ShortLivedCode),
            new("scope", providerDetails.ProviderSetting.Scope),
            new("grant_type", shortLivedAuthorisationDetail.GrantType),
            new("redirect_uri", providerDetails.CallbackRedirectUri),
            new("context", shortLivedAuthorisationDetail.MiscellaneousInfo),
        });
    }
}