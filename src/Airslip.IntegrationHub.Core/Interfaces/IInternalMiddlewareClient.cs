using Airslip.Common.Deletion.Models;
using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Requests;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IInternalMiddlewareClient
{
    Task<IResponse> Authorise(
        ProviderDetails providerDetails,
        MiddlewareAuthorisationRequest middlewareAuthorisationRequest);
    
    Task<IResponse> Delete(
        string accountId, 
        ProviderDetails providerDetails,
        DeleteRequest deleteRequest);
}