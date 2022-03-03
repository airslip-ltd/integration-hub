using Airslip.Common.Types.Enums;
using Microsoft.Azure.Functions.Worker.Http;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IRequestValidationService
{
    bool ValidateRequest(PosProviders parsedProvider, HttpRequestData req);
}