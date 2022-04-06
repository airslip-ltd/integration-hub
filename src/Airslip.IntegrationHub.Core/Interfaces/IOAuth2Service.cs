using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IOAuth2Service
{
    Task<IResponse> ExchangeCodeForAccessToken(string provider, HttpRequestMessage httpRequestMessage);
    
    HttpRequestMessage GetHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail);

    BasicAuthorisationDetail ParseResponseMessage(
        string content,
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail);
}