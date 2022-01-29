using Airslip.Common.Types.Interfaces;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using System;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class AuthorisationService : IAuthorisationService
{
    private readonly IOAuth2Service _oauth2Service;
    private readonly IInternalMiddlewareClient _internalMiddlewareClient;
    private readonly IInternalMiddlewareService _internalMiddlewareService;

    public AuthorisationService(IOAuth2Service oauth2Service, IInternalMiddlewareClient internalMiddlewareClient,
        IInternalMiddlewareService internalMiddlewareService)
    {
        _oauth2Service = oauth2Service;
        _internalMiddlewareClient = internalMiddlewareClient;
        _internalMiddlewareService = internalMiddlewareService;
    }

    public async Task<IResponse> CreateAccount(
        ProviderDetails providerDetails,
        IProviderAuthorisation providerAuthorisingDetail)
    {
        MiddlewareAuthorisationRequest middlewareAuthorisationBody = providerDetails.ProviderSetting.AuthStrategy switch
        {
            ProviderAuthStrategy.ShortLived => await _oauth2Service.QueryPermanentAccessToken(providerDetails, (ShortLivedAuthorisationDetail) providerAuthorisingDetail),
            ProviderAuthStrategy.Basic => _internalMiddlewareService.BuildMiddlewareAuthorisationModel(providerDetails, (BasicAuthorisationDetail) providerAuthorisingDetail),
            ProviderAuthStrategy.Bridge => _internalMiddlewareService.BuildMiddlewareAuthorisationModel(providerDetails, (BasicAuthorisationDetail) providerAuthorisingDetail),
            _ => throw new NotSupportedException()
        };

        return await _internalMiddlewareClient.SendToMiddleware(providerDetails, middlewareAuthorisationBody);
    }
}