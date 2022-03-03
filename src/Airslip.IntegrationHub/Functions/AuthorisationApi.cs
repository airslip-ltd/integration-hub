using Airslip.Common.Auth.Data;
using Airslip.Common.Auth.Functions.Extensions;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
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
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests.GDPR;
using Microsoft.Extensions.Options;

namespace Airslip.IntegrationHub.Functions
{
    public static class AuthorisationApi
    {
        [OpenApiOperation("GenerateAuthorisationUrl", Summary = "The generation of the URL to authorise an OAUTH application")]
        [OpenApiSecurity(AirslipSchemeOptions.ApiKeyScheme, SecuritySchemeType.ApiKey, Name = AirslipSchemeOptions.ApiKeyHeaderField, In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Api Key supplied")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path, Description = "The name of the provider, must be one of our supported providers")]
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

            try
            {
                
                bool supportedProvider = provider.TryParseIgnoreCase(out PosProviders parsedProvider);

                if (!supportedProvider)
                {
                    logger.Warning("{Provider} is an unsupported provider", provider);
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                GenerateUrlDetail generateUrlDetail = req.Url.Query.GetQueryParams<GenerateUrlDetail>();

                HttpResponseData response = req.CreateResponse(HttpStatusCode.Redirect);
                
                if (string.IsNullOrWhiteSpace(req.Url.Query) && generateUrlDetail.TestMode != true)
                {
                    PublicApiSetting uiPublicApiSetting = publicApiSettings.Value.GetSettingByName("UI");
                    response.Headers.Add("Location", uiPublicApiSetting.BaseUri);
                    return response;
                }

                ProviderDetails providerDetails = providerDiscoveryService.GetProviderDetails(parsedProvider, generateUrlDetail.TestMode);
                
                if (!validationService.ValidateRequest(parsedProvider, req))
                {
                    logger.Information("Hmac validation failed for request");
                    return req.CreateResponse(HttpStatusCode.Unauthorized);
                }
                
                IResponse callbackUrl = callbackService.GenerateUrl(providerDetails, req.Url.Query);
                
                if (callbackUrl is not AuthCallbackGeneratorResponse generatedUrl || generateUrlDetail.TestMode)
                    return await req.CommonResponseHandler<AuthCallbackGeneratorResponse>(callbackUrl);
                
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
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path,
            Description = "The name of the provider, must be one of our supported providers")]
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

            try
            {
                bool supportedProvider = provider.TryParseIgnoreCase(out PosProviders parsedProvider);

                if (!supportedProvider)
                {
                    logger.Warning("{Provider} is an unsupported provider", provider);
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                //Validate HMAC another way. Needs improving. Maybe specific to provider.
                // List<KeyValuePair<string, string>> queryStrings = authorisationPreparationService.GetParameters(parsedProvider, req);
                //
                // bool isValid = hmacService.Validate(parsedProvider, queryStrings);
                //
                // if (!isValid)
                // {
                //     logger.Warning("There has been a problem validating the callback request");
                //     return req.CreateResponse(HttpStatusCode.BadRequest);
                // }

                ProviderDetails providerDetails = providerDiscoveryService.GetProviderDetails(parsedProvider);
                IProviderAuthorisation providerAuthorisingDetail = authorisationPreparationService.GetProviderAuthorisationDetail(providerDetails, req);

                if (providerAuthorisingDetail is ErrorAuthorisingDetail errorAuthorisingDetail)
                {
                    HttpResponseData responseData = req.CreateResponse(HttpStatusCode.BadRequest);
                    await responseData.WriteAsJsonAsync(new ErrorResponse(errorAuthorisingDetail.ErrorCode ?? "AuthorisingError", errorAuthorisingDetail.ErrorMessage));
                    return responseData;
                }

                IResponse authorisedResponse = await authorisationService.CreateAccount(providerDetails, providerAuthorisingDetail);

                return await req.CommonResponseHandler<AccountResponse>(authorisedResponse);
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Unhandled error message {ErrorMessage}", e.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [OpenApiOperation("AuthorisationGDPR", Summary = "Process GDPR")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path, Description = "The name of the provider, must be one of our supported providers")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Identity supplied")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid JSON supplied")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        [Function("AuthorisationGDPR")]
        public static async Task<HttpResponseData> GDPR(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/auth/{provider}/gdpr")]
            HttpRequestData req,
            string provider,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            IRequestValidationService validationService = executionContext.InstanceServices
                .GetService<IRequestValidationService>() ?? throw new NotImplementedException();

            try
            {
                bool supportedProvider = provider.TryParseIgnoreCase(out PosProviders parsedProvider);

                if (!supportedProvider)
                {
                    logger.Warning("{Provider} is an unsupported provider", provider);
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }
                
                if (!validationService.ValidateRequest(parsedProvider, req))
                {
                    logger.Information("Hmac validation failed for request");
                    return req.CreateResponse(HttpStatusCode.Unauthorized);
                }

                GDPRRequest gdprRequest = await req.Body.DeserializeStream<GDPRRequest>();

                logger.Information("GDPR request made for {ShopId} with the body {Body}",
                    gdprRequest.ShopId,
                    Json.Serialize(gdprRequest));

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Unhandled error message {ErrorMessage}", e.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}