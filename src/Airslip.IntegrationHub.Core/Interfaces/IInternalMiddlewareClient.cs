using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Requests;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IInternalMiddlewareClient
{
    Task<IResponse> SendToMiddleware(
        ProviderDetails providerDetails,
        MiddlewareAuthorisationRequest middlewareAuthorisationRequest);
}