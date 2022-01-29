using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IInternalMiddlewareService
{
    MiddlewareAuthorisationRequest BuildMiddlewareAuthorisationModel(
        ProviderDetails providerDetails,
        BasicAuthorisationDetail basicAuthorisationDetail);
}