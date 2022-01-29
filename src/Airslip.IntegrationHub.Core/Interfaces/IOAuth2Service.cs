using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using System.Net.Http;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IOAuth2Service
{
    Task<MiddlewareAuthorisationRequest> QueryPermanentAccessToken(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail);
    
    HttpRequestMessage GetHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail);

    BasicAuthorisationDetail ParseResponseMessage(
        string content,
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail);
}