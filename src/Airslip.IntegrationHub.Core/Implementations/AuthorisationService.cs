using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class AuthorisationService : IAuthorisationService
{
    private readonly IOAuth2Service _oauth2Service;
    private readonly IInternalMiddlewareClient _internalMiddlewareClient;
    private readonly IInternalMiddlewareService _internalMiddlewareService;
    private readonly ILogger _logger;

    public AuthorisationService(
        IOAuth2Service oauth2Service, 
        IInternalMiddlewareClient internalMiddlewareClient,
        IInternalMiddlewareService internalMiddlewareService, ILogger logger)
    {
        _oauth2Service = oauth2Service;
        _internalMiddlewareClient = internalMiddlewareClient;
        _internalMiddlewareService = internalMiddlewareService;
        _logger = logger;
    }

    public async Task<IResponse> CreateAccount(
        IntegrationDetails integrationDetails,
        IProviderAuthorisation providerAuthorisingDetail)
    {
        MiddlewareAuthorisationRequest middlewareAuthorisationBody = integrationDetails.IntegrationSetting.AuthStrategy switch
        {
            ProviderAuthStrategy.ShortLived => await _oauth2Service.QueryPermanentAccessToken(providerDetails, (ShortLivedAuthorisationDetail) providerAuthorisingDetail),
            ProviderAuthStrategy.Basic => _internalMiddlewareService.BuildMiddlewareAuthorisationModel(providerDetails, (BasicAuthorisationDetail) providerAuthorisingDetail),
            ProviderAuthStrategy.Bridge => _internalMiddlewareService.BuildMiddlewareAuthorisationModel(providerDetails, (BasicAuthorisationDetail) providerAuthorisingDetail),
            _ => throw new NotSupportedException()
        };

        if (middlewareAuthorisationBody.Failed)
            return new ErrorResponse("MIDDLEWARE_ERROR", "Error exchanging token");

        _logger.Information("Sending to middleware {Middleware} with body {Body}", 
            integrationDetails.IntegrationSetting.PublicApiSettingName, 
            Json.Serialize(middlewareAuthorisationBody));
        
        return await _internalMiddlewareClient.Authorise(providerDetails, middlewareAuthorisationBody);
    }
}