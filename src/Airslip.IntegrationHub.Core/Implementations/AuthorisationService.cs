using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using Microsoft.Azure.Functions.Worker.Http;
using Serilog;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class AuthorisationService : IAuthorisationService
{
    private readonly IOAuth2Service _oauth2Service;
    private readonly IInternalMiddlewareClient _internalMiddlewareClient;
    private readonly IInternalMiddlewareService _internalMiddlewareService;
    private readonly IAuthorisationPreparationService _authorisationPreparationService;
    private readonly ILogger _logger;
    private readonly IIntegrationDiscoveryService _discoveryService;

    public AuthorisationService(
        IOAuth2Service oauth2Service, 
        IInternalMiddlewareClient internalMiddlewareClient,
        IInternalMiddlewareService internalMiddlewareService, 
        ILogger logger, 
        IAuthorisationPreparationService authorisationPreparationService,
        IIntegrationDiscoveryService discoveryService)
    {
        _oauth2Service = oauth2Service;
        _internalMiddlewareClient = internalMiddlewareClient;
        _internalMiddlewareService = internalMiddlewareService;
        _logger = logger;
        _authorisationPreparationService = authorisationPreparationService;
        _discoveryService = discoveryService;
    }

    public async Task<IResponse> CreateAccount(
        HttpRequestData req,
        string provider)
    {
        Dictionary<string, string> parameters = _authorisationPreparationService.GetParameters(req);

        SensitiveCallbackInfo sensitiveInfo = _authorisationPreparationService.TransformParametersToSensitiveCallbackInfo(parameters)!;

        IntegrationDetails integrationDetails = _discoveryService.GetIntegrationDetails(
            provider, 
            sensitiveInfo.IntegrationProviderId,
            sensitiveInfo.TestMode);

        if (integrationDetails.IntegrationSetting.AuthStrategy == ProviderAuthStrategy.ShortLived)
        {
            HttpRequestMessage httpRequestMessage = _authorisationPreparationService.GetHttpRequestMessageForAccessToken(integrationDetails, parameters);
            
            IResponse accessTokenResponse = await _oauth2Service.ExchangeCodeForAccessToken(provider, httpRequestMessage);
            if (accessTokenResponse is AccessTokenModel accessTokenModel)
                parameters = accessTokenModel.Parameters;
            else
                return accessTokenResponse;
        }

        BasicAuthorisationDetail basicAuthorisationDetail = _authorisationPreparationService
            .BuildSuccessfulAuthorisationModel(integrationDetails, parameters);
        
        MiddlewareAuthorisationRequest r = _internalMiddlewareService.BuildMiddlewareAuthorisationModel(
            provider,
            integrationDetails,
            sensitiveInfo,
            basicAuthorisationDetail);

        // MiddlewareAuthorisationRequest middlewareAuthorisationBody = providerDetails.ProviderSetting.AuthStrategy switch
        // {
        //     ProviderAuthStrategy.ShortLived => await _oauth2Service.QueryPermanentAccessToken(providerDetails, (ShortLivedAuthorisationDetail) providerAuthorisingDetail),
        //     ProviderAuthStrategy.Basic => _internalMiddlewareService.BuildMiddlewareAuthorisationModel(providerDetails, (BasicAuthorisationDetail) providerAuthorisingDetail),
        //     ProviderAuthStrategy.Bridge => _internalMiddlewareService.BuildMiddlewareAuthorisationModel(providerDetails, (BasicAuthorisationDetail) providerAuthorisingDetail),
        //     _ => throw new NotSupportedException()
        // };

        if (r.GetType() != typeof(MiddlewareAuthorisationRequest))
            return new ErrorResponse("MIDDLEWARE_ERROR", "Error exchanging token");

        _logger.Information("Sending to middleware {Middleware} with body {Body}", 
            integrationDetails.IntegrationSetting.PublicApiSettingName, 
            Json.Serialize(r));
        
        return await _internalMiddlewareClient.Authorise(provider, integrationDetails, r);
    }
}