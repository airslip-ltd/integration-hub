using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities.Extensions;
using Airslip.Common.Utilities.Models;
using Airslip.IntegrationHub.Core.Enums;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Responses;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Airslip.IntegrationHub.Core.Common.Discovery
{
    public class IntegrationUrlService : IIntegrationUrlService
    {
        private readonly IIntegrationDiscoveryService _discoveryService;
        private readonly HttpClient _httpClient;
        private readonly IAuthorisationPreparationService _authorisationPreparationService;
        private readonly IOAuth2Service _oauth2Service;
        private readonly IInternalMiddlewareClient _internalMiddlewareClient;
        private readonly IInternalMiddlewareService _internalMiddlewareService;

        public IntegrationUrlService(
            IHttpClientFactory factory,
            IIntegrationDiscoveryService discoveryService,
            IAuthorisationPreparationService authorisationPreparationService,
            IOAuth2Service oauth2Service,
            IInternalMiddlewareClient internalMiddlewareClient,
            IInternalMiddlewareService internalMiddlewareService)
        {
            _httpClient = factory.CreateClient(nameof(IntegrationUrlService));
            _discoveryService = discoveryService;
            _authorisationPreparationService = authorisationPreparationService;
            _oauth2Service = oauth2Service;
            _internalMiddlewareClient = internalMiddlewareClient;
            _internalMiddlewareService = internalMiddlewareService;
        }

        public async Task<IResponse> GetAuthorisationUrl(
            string provider,
            SensitiveCallbackInfo sensitiveCallbackInfo,
            CancellationToken cancellationToken)
        {
            IntegrationDetails integrationDetails = _discoveryService.GetIntegrationDetails(
                provider,
                sensitiveCallbackInfo.IntegrationProviderId,
                sensitiveCallbackInfo.TestMode);

            string url = $"{integrationDetails.Uri}/{integrationDetails.IntegrationSetting.AuthorisationRouteFormat}";
            Dictionary<string, string> replacements = new();
            // Apply some dynamic replacement
            replacements.Add("entityId", sensitiveCallbackInfo.EntityId);
            replacements.Add("airslipUserType", sensitiveCallbackInfo.AirslipUserType.ToString().ToLower());
            replacements.Add("userId", sensitiveCallbackInfo.UserId);
            replacements.Add("provider", provider);
            if (sensitiveCallbackInfo.IntegrationProviderId != null)
                replacements.Add("integrationProviderId", sensitiveCallbackInfo.IntegrationProviderId);

            if (!string.IsNullOrEmpty(sensitiveCallbackInfo.Shop))
                replacements.Add("shop", sensitiveCallbackInfo.Shop);

            if (!string.IsNullOrEmpty(sensitiveCallbackInfo.CipheredSensitiveInfo))
                replacements.Add("state", sensitiveCallbackInfo.CipheredSensitiveInfo);

            if (!string.IsNullOrEmpty(integrationDetails.IntegrationSetting.ApiKey))
                replacements.Add("apiKey", integrationDetails.IntegrationSetting.ApiKey);

            if (!string.IsNullOrEmpty(integrationDetails.IntegrationSetting.AppName))
                replacements.Add("appName", integrationDetails.IntegrationSetting.AppName);

            if (!string.IsNullOrEmpty(integrationDetails.IntegrationSetting.Version))
                replacements.Add("version", integrationDetails.IntegrationSetting.Version);

            if (!string.IsNullOrEmpty(integrationDetails.IntegrationSetting.ReturnPageFormat))
                replacements.Add("returnPage", integrationDetails.IntegrationSetting.ReturnPage);

            if (!string.IsNullOrEmpty(integrationDetails.IntegrationSetting.Scope))
            {
                string scope = integrationDetails.IntegrationSetting.RequireUrlEncode
                    ? HttpUtility.UrlEncode(integrationDetails.IntegrationSetting.Scope)
                    : integrationDetails.IntegrationSetting.Scope;

                replacements.Add("scope", scope);
            }

            if (!string.IsNullOrEmpty(sensitiveCallbackInfo.CallbackUrl))
            {
                replacements.Add("callbackUrl", sensitiveCallbackInfo.CallbackUrl);
            }
            else if (!string.IsNullOrEmpty(integrationDetails.CallbackUrl))
            {
                string callbackUrl = integrationDetails.IntegrationSetting.RequireUrlEncode
                    ? HttpUtility.UrlEncode(integrationDetails.CallbackUrl)
                    : integrationDetails.CallbackUrl;

                replacements.Add("callbackUrl", callbackUrl);
            }

            url = url.ApplyReplacements(replacements);

            IResponse? result;

            if (integrationDetails.IntegrationSetting.OAuthRedirect)
            {
                result = new AuthorisationResponse(url);
            }
            else
            {
                HttpActionResult apiCallResponse = await _httpClient
                    .GetApiRequest<AuthorisationResponse>(url, integrationDetails.ApiKey, cancellationToken);

                result = apiCallResponse.StatusCode switch
                {
                    HttpStatusCode.OK => apiCallResponse.Response!,
                    HttpStatusCode.BadRequest => apiCallResponse.Response ??
                                                 new ErrorResponse("BadRequest", apiCallResponse.Content),
                    HttpStatusCode.Unauthorized => new UnauthorisedResponse(provider, "Unauthenticated"),
                    _ => new ErrorResponse("BadRequest", apiCallResponse.Content)
                };
            }

            return result;
        }

        public async Task<IResponse> ApproveIntegration(
            HttpRequestData req,
            string provider,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> parameters = _authorisationPreparationService.GetParameters(req);

            SensitiveCallbackInfo sensitiveInfo =
                _authorisationPreparationService.TransformParametersToSensitiveCallbackInfo(parameters)!;

            IntegrationDetails integrationDetails = sensitiveInfo is null
                ? _discoveryService.GetIntegrationDetails(provider)
                : _discoveryService.GetIntegrationDetails(
                    provider,
                    sensitiveInfo.IntegrationProviderId,
                    sensitiveInfo.TestMode);

            if (integrationDetails.IntegrationSetting.AuthStrategy == ProviderAuthStrategy.ShortLived)
            {
                HttpRequestMessage httpRequestMessage =
                    _authorisationPreparationService.GetHttpRequestMessageForAccessToken(
                        integrationDetails,
                        parameters);

                IResponse accessTokenResponse =
                    await _oauth2Service.ExchangeCodeForAccessToken(provider, httpRequestMessage);
                if (accessTokenResponse is AccessTokenModel accessTokenModel)
                    parameters = accessTokenModel.Parameters;
                else
                    return accessTokenResponse;
            }

            // TODO: @GrahamWhitehoiuse is this needed?
            //if (integrationDetails.IntegrationSetting.AuthorisePassthrough)
            //    url =
            //        $"{url}{(url.IndexOf("?") < 0 ? "?" : "")}{string.Join("&", replacements.Select(o => $"{o.Key}={HttpUtility.UrlEncode(o.Value)}"))}";

            //if (integrationDetails.IntegrationSetting.AnonymousUsage)
            //    url += $"&user_info={sensitiveInfo?.CipheredSensitiveInfo}";

            //// Get successful auth values and put into a common model
            //string url = _authorisationPreparationService.GenerateMiddlewareDestinationUrl(provider, integrationDetails, parameters, sensitiveInfo!);

            // Get successful auth values and put into a common model
            string url = _authorisationPreparationService.GenerateMiddlewareDestinationUrl(provider, integrationDetails, parameters, sensitiveInfo!);

            // Send to internal middleware
            if (integrationDetails.IntegrationSetting.PublicApiMethodType == MethodTypes.POST)
                return await _internalMiddlewareClient.Authorise(integrationDetails.IntegrationSetting.PublicApiSetting.ApiKey, url);

            HttpActionResult apiCallResponse = await _httpClient.GetApiRequest<IntegrationResponse>(url, integrationDetails.ApiKey, cancellationToken);
            return apiCallResponse.StatusCode switch
            {
                HttpStatusCode.OK => apiCallResponse.Response!,
                HttpStatusCode.BadRequest => apiCallResponse.Response ??
                                             new ErrorResponse("BadRequest", apiCallResponse.Content),
                HttpStatusCode.Unauthorized => new UnauthorisedResponse(provider, "Unauthenticated"),
                _ => new ErrorResponse("BadRequest", apiCallResponse.Content)
            };
        }
    }
}