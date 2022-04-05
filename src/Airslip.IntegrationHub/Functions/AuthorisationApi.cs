using Airslip.Common.Auth.Data;
using Airslip.Common.Auth.Functions.Extensions;
using Airslip.Common.Functions.Interfaces;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Responses;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Enums;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading;

namespace Airslip.IntegrationHub.Functions
{
    public static class AuthorisationApi
    {
        [OpenApiOperation("GenerateAuthorisationUrl", Summary = "The generation of the URL to authorise an OAUTH application")]
        [OpenApiSecurity(AirslipSchemeOptions.ApiKeyScheme, SecuritySchemeType.ApiKey, Name = AirslipSchemeOptions.ApiKeyHeaderField, In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Api Key supplied")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path, Description = "The name of the provider, must be one of our supported providers")]
        [OpenApiParameter("integrationType", Required = true, In = ParameterLocation.Path, Description = "The name of the integration type, must be one of our supported integrations")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse), Description = "Invalid JSON supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, Json.MediaType, typeof(string), Description = "The URL to be used to start an external authorisation process")]
        [Function("GenerateAuthorisationUrl")]
        public static async Task<HttpResponseData> GenerateAuthorisationUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/auth/{provider}")]
            HttpRequestData req,
            FunctionContext executionContext,
            string provider)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            ICallbackService callbackService = executionContext.InstanceServices.GetService<ICallbackService>() ?? throw new NotImplementedException();
            IRequestValidationService validationService = executionContext.InstanceServices.GetService<IRequestValidationService>() ?? throw new NotImplementedException();
            IOptions<PublicApiSettings> publicApiSettings = executionContext.InstanceServices.GetService<IOptions<PublicApiSettings>>() ?? throw new NotImplementedException();
            IProviderDiscoveryService providerDiscoveryService = executionContext.InstanceServices.GetService<IProviderDiscoveryService>() ?? throw new NotImplementedException();
            IFunctionApiTools functionApiTools = executionContext.InstanceServices.GetService<IFunctionApiTools>() ?? throw new NotImplementedException();
            ISensitiveInformationService sensitiveInformationService = executionContext.InstanceServices.GetService<ISensitiveInformationService>() ?? throw new NotImplementedException();
            IIntegrationUrlService integrationUrlService = executionContext.InstanceServices.GetService<IIntegrationUrlService>() ?? throw new NotImplementedException();

            try
            {
                // Need to change name to something more appropriate
                SensitiveCallbackInfo sensitiveCallbackInfo = sensitiveInformationService.DeserializeSensitiveInfoQueryString(req.Url.Query);
                
                // WIP test
                if (true)
                {
                    IResponse r = await integrationUrlService.GetAuthorisationUrl(provider, sensitiveCallbackInfo, CancellationToken.None);
                    return await functionApiTools.CommonResponseHandler<AuthorisationResponse>(req, r);
                }

                ProviderDetails? providerDetails = providerDiscoveryService.GetPosProviderDetails(provider, sensitiveCallbackInfo.TestMode);

                if (providerDetails is null)
                {
                    logger.Warning("{Provider} is an unsupported provider", provider);
                    return await functionApiTools.BadRequest(req, new InvalidAttribute(nameof(provider), provider, $"{provider} is an unsupported provider"));
                }

                HttpResponseData response = req.CreateResponse(HttpStatusCode.Redirect);

                // This is in place to cater for the app redirects from third party providers
                //  the idea here is that the user will hit the UI hompage where they click on our
                //  app link within the third party tool
                if (string.IsNullOrWhiteSpace(req.Url.Query) && sensitiveCallbackInfo.TestMode != true)
                {
                    PublicApiSetting uiPublicApiSetting = publicApiSettings.Value.GetSettingByName("UI");
                    response.Headers.Add("Location", uiPublicApiSetting.BaseUri);
                    return response;
                }

                if (!validationService.ValidateRequest(providerDetails, req, AuthRequestTypes.Generate))
                {
                    logger.Information("Hmac validation failed for request");
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    return await functionApiTools.Unauthorised(req,
                        new UnauthorisedResponse(provider, "Hmac validation failed for request"));
                }

                if (providerDetails.ProviderSetting.ValidateIfRequiresStoreName(sensitiveCallbackInfo.Shop))
                {
                    logger.Information("{Provider} requires a shop name", provider);
                    return await functionApiTools.Unauthorised(req,
                        new UnauthorisedResponse(provider, $"{provider} requires a shop name"));
                }

                IResponse callbackUrl = callbackService.GenerateUrl(providerDetails, sensitiveCallbackInfo);

                if (callbackUrl is not AuthorisationResponse generatedUrl || sensitiveCallbackInfo.TestMode)
                    return await functionApiTools.CommonResponseHandler<AuthorisationResponse>(req, callbackUrl);

                response.Headers.Add("Location", generatedUrl.AuthorisationUrl);
                return response;
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Unhandled error message {ErrorMessage}", e.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [OpenApiOperation("AuthorisationCallback", Summary = "Callback to authorise a service with using OAUTH")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path, Description = "The name of the provider, must be one of our supported providers")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse),
            Description = "Invalid JSON supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, Json.MediaType, typeof(AccountResponse),
            Description = "Details of the account that has been setup")]
        [Function("AuthorisationCallback")]
        public static async Task<HttpResponseData> Callback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "v1/auth/callback/{provider}")]
            HttpRequestData req,
            string provider,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            IProviderDiscoveryService providerDiscoveryService = executionContext.InstanceServices.GetService<IProviderDiscoveryService>() ?? throw new NotImplementedException();
            IHmacService hmacService = executionContext.InstanceServices.GetService<IHmacService>() ?? throw new NotImplementedException();
            IAuthorisationPreparationService authorisationPreparationService = executionContext.InstanceServices.GetService<IAuthorisationPreparationService>() ?? throw new NotImplementedException();
            IAuthorisationService authorisationService = executionContext.InstanceServices.GetService<IAuthorisationService>() ?? throw new NotImplementedException();
            IFunctionApiTools functionApiTools = executionContext.InstanceServices.GetService<IFunctionApiTools>() ?? throw new NotImplementedException();
            ISensitiveInformationService sensitiveInformationService = executionContext.InstanceServices.GetService<ISensitiveInformationService>() ?? throw new NotImplementedException();

            try
            {
                ProviderDetails? providerDetails = providerDiscoveryService.GetPosProviderDetails(provider);

                if (providerDetails is null)
                {
                    logger.Warning("{Provider} is an unsupported provider", provider);
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                //Validate HMAC another way. Needs improving. Maybe specific to provider.
                // List<KeyValuePair<string, string>> queryStrings = authorisationPreparationService.GetParameters(parsedProvider, req);
                //
                // bool isValid = hmacService.Validate(providerDetails, queryStrings);
                //
                // if (!isValid)
                // {
                //     logger.Warning("There has been a problem validating the callback request");
                //     return req.CreateResponse(HttpStatusCode.BadRequest);
                // }

                IProviderAuthorisation providerAuthorisingDetail = authorisationPreparationService.GetProviderAuthorisationDetail(providerDetails, req);

                if (providerAuthorisingDetail is ErrorAuthorisingDetail errorAuthorisingDetail)
                {
                    HttpResponseData responseData = req.CreateResponse(HttpStatusCode.BadRequest);
                    await responseData.WriteAsJsonAsync(new ErrorResponse(errorAuthorisingDetail.ErrorCode ?? "AuthorisingError", errorAuthorisingDetail.ErrorMessage));
                    return responseData;
                }

                // Need to refactor. I need provider details to deserialize content for a post and a get but I need Sensitive Info 
                if (providerAuthorisingDetail.SensitiveCallbackInfo.TestMode)
                    providerDetails = providerDiscoveryService.GetPosProviderDetails(provider, providerAuthorisingDetail.SensitiveCallbackInfo.TestMode);

                IResponse authorisedResponse = await authorisationService.CreateAccount(providerDetails!, providerAuthorisingDetail);
                return await functionApiTools.CommonResponseHandler<AccountResponse>(req, authorisedResponse);
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Unhandled error message {ErrorMessage}", e.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}