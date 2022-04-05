using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Interfaces;

public interface IAuthorisationPreparationService
{
    IProviderAuthorisation GetProviderAuthorisationDetail(
        HttpRequestData req,
        string provider);

    List<KeyValuePair<string, string>> GetParameters(
        string provider,
        HttpRequestData req);
}