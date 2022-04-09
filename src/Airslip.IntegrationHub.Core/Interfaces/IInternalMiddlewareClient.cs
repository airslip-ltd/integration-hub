using Airslip.Common.Deletion.Models;
using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Requests;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IInternalMiddlewareClient
{
    Task<IResponse> Authorise(
        string provider,
        IntegrationDetails integrationDetails,
        MiddlewareAuthorisationRequest middlewareAuthorisationRequest);

    Task<IResponse> Delete(
        string accountId, 
        string provider, 
        IntegrationDetails integrationDetails,
        DeleteRequest deleteRequest);
}