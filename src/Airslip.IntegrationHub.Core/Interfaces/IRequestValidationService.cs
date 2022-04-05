using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IRequestValidationService
{
    IResponse ValidateRequest(
        HttpRequestData req,
        string provider,
        AuthRequestTypes authRequestType);
}