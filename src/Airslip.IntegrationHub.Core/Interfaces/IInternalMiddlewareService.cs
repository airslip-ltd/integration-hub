using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IInternalMiddlewareService
{
    MiddlewareAuthorisationRequest BuildMiddlewareAuthorisationModel(
        string provider,
        IntegrationDetails integrationDetails,
        SensitiveCallbackInfo sensitiveCallbackInfo,
        BasicAuthorisationDetail basicAuthorisationDetail);
}