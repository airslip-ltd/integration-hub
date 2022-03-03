using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IRequestValidationService
{
    bool ValidateRequest(
        ProviderDetails providerDetails,
        HttpRequestData req,
        AuthRequestTypes authRequestType);
}