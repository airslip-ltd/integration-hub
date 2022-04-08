using Airslip.Common.Security.Implementations;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.Common.Utilities.Models;
using Airslip.IntegrationHub.Core.Enums;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using Airslip.IntegrationHub.Core.Responses;
using Microsoft.Azure.Functions.Worker.Http;
using Serilog;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ILogger _logger;

        public IntegrationUrlService(
            IHttpClientFactory factory,
            IIntegrationDiscoveryService discoveryService,
            IAuthorisationPreparationService authorisationPreparationService, 
            IOAuth2Service oauth2Service,
            IInternalMiddlewareClient internalMiddlewareClient, 
            IInternalMiddlewareService internalMiddlewareService,
            ILogger logger)
        {
            _httpClient = factory.CreateClient(nameof(IntegrationUrlService));
            _discoveryService = discoveryService;
            _authorisationPreparationService = authorisationPreparationService;
            _oauth2Service = oauth2Service;
            _internalMiddlewareClient = internalMiddlewareClient;
            _internalMiddlewareService = internalMiddlewareService;
            _logger = logger;
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
            
            _logger.Information("ApiKey for {Provider} is {ApiKey}", provider, integrationDetails.IntegrationSetting.ApiKey);

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

            if (!string.IsNullOrEmpty(integrationDetails.CallbackUrl))
            {
                string callbackUrl = integrationDetails.IntegrationSetting.RequireUrlEncode
                    ? HttpUtility.UrlEncode(integrationDetails.CallbackUrl)
                    : integrationDetails.CallbackUrl;

                replacements.Add("callbackUrl", callbackUrl);
            }

            url = url.ApplyReplacements(replacements);
            
            _logger.Information("Url for {Provider} is {Url}", provider, url);

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
                    _authorisationPreparationService.GetHttpRequestMessageForAccessToken(integrationDetails,
                        parameters);

                IResponse accessTokenResponse =
                    await _oauth2Service.ExchangeCodeForAccessToken(provider, httpRequestMessage);
                if (accessTokenResponse is AccessTokenModel accessTokenModel)
                    parameters = accessTokenModel.Parameters;
                else
                    return accessTokenResponse;
            }

            if (integrationDetails.IntegrationSetting.IntegrationType == IntegrationTypes.Commerce)
            {
                BasicAuthorisationDetail basicAuthorisationDetail = _authorisationPreparationService
                    .BuildSuccessfulAuthorisationModel(integrationDetails, parameters);

                MiddlewareAuthorisationRequest r = _internalMiddlewareService.BuildMiddlewareAuthorisationModel(
                    provider,
                    integrationDetails,
                    sensitiveInfo!,
                    basicAuthorisationDetail);

                return await _internalMiddlewareClient.Authorise(provider, integrationDetails, r);
            }

            Dictionary<string, string> replacements = _authorisationPreparationService.BankingQueryStringReplacer(parameters);

            string relativeUrl =
                integrationDetails.IntegrationSetting.AuthoriseRouteFormat.ApplyReplacements(replacements);
            string url = $"{integrationDetails.Uri}/{relativeUrl}";

            if (integrationDetails.IntegrationSetting.AnonymousUsage)
            {
                if (integrationDetails.IntegrationSetting.AuthorisePassthrough)
                    url =
                        $"{url}{(url.IndexOf("?") < 0 ? "?" : "")}{string.Join("&", replacements.Select(o => $"{o.Key}={HttpUtility.UrlEncode(o.Value)}"))}";

                url = $"{url}&user_info={sensitiveInfo?.CipheredSensitiveInfo}";
            }
            else if (integrationDetails.IntegrationSetting.AuthorisePassthrough)
                url =
                    $"{url}{(url.IndexOf("?") < 0 ? "?" : "")}{string.Join("&", replacements.Select(o => $"{o.Key}={HttpUtility.UrlEncode(o.Value)}"))}";


            HttpActionResult apiCallResponse = await _httpClient
                .GetApiRequest<AccountResponse>(url, integrationDetails.ApiKey, cancellationToken);

            return apiCallResponse.StatusCode switch
            {
                HttpStatusCode.OK => apiCallResponse.Response!,
                HttpStatusCode.BadRequest => apiCallResponse.Response ??
                                             new ErrorResponse("BadRequest", apiCallResponse.Content),
                HttpStatusCode.Unauthorized => new UnauthorisedResponse(provider, "Unauthenticated"),
                _ => new ErrorResponse("BadRequest", apiCallResponse.Content)
            };
        }

        // public async Task<IResponse> DeleteIntegration(string integrationId, CancellationToken cancellationToken)
        // {
        //     Dictionary<string, string> replacements = new();
        //     Integration? integration = await _context.GetEntity<Integration>(integrationId);
        //     if (integration == null) return new NotFoundResponse("Integration", integrationId);
        //     
        //     IntegrationProvider? integrationProvider = await _context
        //         .GetEntity<IntegrationProvider>(integration.IntegrationProviderId);
        //     if (integrationProvider == null) return new NotFoundResponse("integrationProvider", integration.IntegrationProviderId);
        //
        //     IntegrationDetails integrationDetails = _discoveryService.GetIntegrationDetails(integrationProvider.Provider, 
        //         integrationProvider.Id);
        //
        //     string url = $"{integrationDetails.Uri}/delete/{integration}/{integrationId}";
        //     
        //     replacements.Add("integration", integrationProvider.Id);
        //     replacements.Add("integrationId", integrationId);
        //     
        //     url = url.ApplyReplacements(replacements);
        //     
        //     HttpActionResult apiCallResponse = await _httpClient
        //         .ApiRequestWithBody<DeleteIntegrationResponse, DeleteIntegrationRequest>(url, integrationDetails.ApiKey,
        //             new DeleteIntegrationRequest(_userToken.UserId, _userToken.EntityId, _userToken.AirslipUserType), 
        //             HttpMethod.Delete,
        //             cancellationToken);
        //
        //     return apiCallResponse.StatusCode switch
        //     {
        //         HttpStatusCode.OK => apiCallResponse.Response!,
        //         HttpStatusCode.BadRequest => apiCallResponse.Response ?? 
        //                                      new ErrorResponse("BadRequest", apiCallResponse.Content),
        //         HttpStatusCode.Unauthorized => new UnauthorisedResponse(integrationProvider.Provider, "Unauthenticated"),
        //         _ => new ErrorResponse("BadRequest", apiCallResponse.Content)
        //     };
        // }
        //
        // private string GetEncryptedUserInformation(SensitiveCallbackInfo info, string passPhraseToken)
        // {
        //     string serialisedUserInformation = Json.Serialize(info);
        //
        //     string cipheredSensitiveInfo = StringCipher.EncryptForUrl(
        //         serialisedUserInformation,
        //         passPhraseToken);
        //
        //     return cipheredSensitiveInfo;
        // }

        // private SensitiveCallbackInfo _decodeCallbackInfo(Dictionary<string, string> queryParams)
        // {
        //     SensitiveCallbackInfo info;
        //     string shopName = queryParams.ContainsKey("shop") ? queryParams["shop"] : string.Empty; 
        //     
        //     if (queryParams.ContainsKey("state"))
        //     {
        //         string decryptedData = StringCipher.Decrypt(queryParams["state"], 
        //             _tokenEncryptionSettings.PassPhraseToken);
        //
        //         info = Json.Deserialize<SensitiveCallbackInfo>(decryptedData);
        //     }
        //     else
        //     {
        //         info = new SensitiveCallbackInfo
        //         {
        //             AirslipUserType = _userToken.AirslipUserType,
        //             EntityId = _userToken.EntityId,
        //             UserId = _userToken.UserId,
        //             Shop = shopName
        //         };
        //     }
        //
        //     return info;
        // }
    }
}