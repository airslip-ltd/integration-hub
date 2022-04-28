using Airslip.Common.Types;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Enums;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Implementations;

public class RequestValidationService : IRequestValidationService
{
    private readonly IAuthorisationPreparationService _authorisationPreparation;
    private readonly IHmacService _hmacService;
    private readonly IIntegrationDiscoveryService _discoveryService;
    private readonly ISensitiveInformationService _sensitiveInformationService;

    public RequestValidationService(
        IAuthorisationPreparationService authorisationPreparation,
        IHmacService hmacService,
        IIntegrationDiscoveryService discoveryService,
        ISensitiveInformationService sensitiveInformationService)
    {
        _authorisationPreparation = authorisationPreparation;
        _hmacService = hmacService;
        _discoveryService = discoveryService;
        _sensitiveInformationService = sensitiveInformationService;
    }

    public IResponse ValidateRequest(
        HttpRequestData req,
        string provider,
        AuthRequestTypes authRequestType)
    {
        Dictionary<string, string> parameters = _authorisationPreparation.GetParameters(req);

        IntegrationDetails integrationDetails = _discoveryService.GetIntegrationDetails(provider);
        if (integrationDetails is IntegrationNotFound)
            return new NotFoundResponse(nameof(provider), provider);
        
        // May need to validate 
        if(integrationDetails.IntegrationSetting.IntegrationType == IntegrationTypes.Banking)
            return Success.Instance;

        SensitiveCallbackInfo? sensitiveCallbackInfo = authRequestType == AuthRequestTypes.Generate
            ? _sensitiveInformationService.DeserializeQueryString(req.Url.Query)
            : _authorisationPreparation.TransformParametersToSensitiveCallbackInfo(parameters);

        if (sensitiveCallbackInfo is null)
            return new NotFoundResponse("state", "Unable to find state");

        integrationDetails = _discoveryService.GetIntegrationDetails(
            provider,
            sensitiveCallbackInfo.IntegrationProviderId,
            sensitiveCallbackInfo.TestMode);

        if (integrationDetails.IntegrationSetting.IsNotSupported())
            return new NotFoundResponse(nameof(provider), $"{provider} is not supported");

        if (integrationDetails.IntegrationSetting.ValidateIfRequiresStoreName(sensitiveCallbackInfo.Shop))
            return new UnauthorisedResponse(provider, $"{provider} requires a shop name");

        if (integrationDetails.IntegrationSetting.ShouldValidateHmac(authRequestType))
        {
            bool isValid =
                _hmacService.Validate(provider, integrationDetails.IntegrationSetting.ApiSecret, parameters);
            if (!isValid)
            {
                return new UnauthorisedResponse(provider, "Hmac validation failed for request");
            }
        }

        return Success.Instance;
    }
}