using Airslip.Common.Types;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Common.Discovery;
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
        SensitiveCallbackInfo sensitiveCallbackInfo =
            _sensitiveInformationService.DeserializeSensitiveInfoQueryString(req.Url.Query);

        IntegrationDetails integrationDetails = _discoveryService.GetIntegrationDetails(
            provider,
            sensitiveCallbackInfo.IntegrationProviderId,
            sensitiveCallbackInfo.TestMode);

        if (integrationDetails.IntegrationSetting.IsNotSupported())
            return new NotFoundResponse(nameof(provider), $"{provider} is not supported");

        if (integrationDetails.IntegrationSetting.ValidateIfRequiresStoreName(sensitiveCallbackInfo.Shop))
            return new UnauthorisedResponse(provider, $"{provider} requires a shop name");

        if (integrationDetails.IntegrationSetting.ShouldValidateHmac(authRequestType))
        {
            List<KeyValuePair<string, string>> queryStrings = _authorisationPreparation.GetParameters(provider, req);

            bool isValid =
                _hmacService.Validate(provider, integrationDetails.IntegrationSetting.ApiSecret, queryStrings);
            if (!isValid)
            {
                return new UnauthorisedResponse(provider, "Hmac validation failed for request");
            }
        }

        return Success.Instance;
    }
}