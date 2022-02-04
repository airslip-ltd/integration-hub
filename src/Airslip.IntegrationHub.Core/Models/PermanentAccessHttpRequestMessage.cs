using Airslip.Common.Utilities;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Airslip.IntegrationHub.Core.Models;

public abstract class PermanentAccessHttpRequestMessage : HttpRequestMessage
{
    protected PermanentAccessHttpRequestMessage(
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail)
    {
        RequestUri = new Uri(shortLivedAuthorisationDetail.PermanentAccessUrl);
    }

    protected AuthenticationHeaderValue BuildBasicAuth(string apiId, string apiSecret)
    {
        return new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiId}:{apiSecret}")));
    }

    protected HttpContent BuildStringContent(PermanentAccessBase permanentAccessBase)
    {
        return new StringContent(
            Json.Serialize(permanentAccessBase),
            Encoding.UTF8,
            Json.MediaType);
    }
}