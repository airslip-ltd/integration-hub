using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Implementations;

public class RequestValidationService : IRequestValidationService
{
    private readonly IAuthorisationPreparationService _authorisationPreparation;
    private readonly IHmacService _hmacService;

    public RequestValidationService(
        IAuthorisationPreparationService authorisationPreparation, 
        IHmacService hmacService)
    {
        _authorisationPreparation = authorisationPreparation;
        _hmacService = hmacService;
    }
    
    public bool ValidateRequest(
        ProviderDetails providerDetails, 
        HttpRequestData req, 
        AuthRequestTypes authRequestType)
    {
        if (!providerDetails.ProviderSetting.ShouldValidate(authRequestType))
            return true;

        List<KeyValuePair<string, string>> queryStrings = _authorisationPreparation.GetParameters(providerDetails, req);
                
        return _hmacService.Validate(providerDetails, queryStrings);
    }
}