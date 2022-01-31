using Airslip.Common.Types.Enums;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Services;

public interface IAuthorisationPreparationService
{
    IProviderAuthorisation GetProviderAuthorisationDetail(
        ProviderDetails providerDetails,
        HttpRequestData req);
    
    List<KeyValuePair<string, string>> GetParameters(
        PosProviders provider,
        HttpRequestData req);
}