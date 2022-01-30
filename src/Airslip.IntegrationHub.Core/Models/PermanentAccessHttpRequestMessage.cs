using System;
using System.Net.Http;

namespace Airslip.IntegrationHub.Core.Models;

public abstract class PermanentAccessHttpRequestMessage : HttpRequestMessage
{
    protected PermanentAccessHttpRequestMessage(
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail)
    {
        Method = HttpMethod.Post;
        RequestUri = new Uri(shortLivedAuthorisationDetail.PermanentAccessUrl);
    }
}