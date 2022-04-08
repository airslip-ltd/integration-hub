using Airslip.Common.Types.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IOAuth2Service
{
    Task<IResponse> ExchangeCodeForAccessToken(string provider, HttpRequestMessage httpRequestMessage);
}