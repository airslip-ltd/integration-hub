using Airslip.Common.Auth.Data;
using Airslip.Common.Auth.Functions.Attributes;
using Airslip.Common.Auth.Functions.Extensions;
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
using Airslip.IntegrationHub.Core.Models.ThreeDCart;
using Airslip.IntegrationHub.Services;
using System.Collections.Generic;
using System.Linq;

namespace Airslip.IntegrationHub.Functions
{
    public static class AuthorisationApi
    {
        [OpenApiOperation("GenerateAuthorisationUrl",
            Summary = "The generation of the URL to authorise an OAUTH application")]
        [OpenApiSecurity(AirslipSchemeOptions.ApiKeyScheme, SecuritySchemeType.ApiKey,
            Name = AirslipSchemeOptions.ApiKeyHeaderField, In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Api Key supplied")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path,
            Description = "The name of the provider, must be one of our supported providers")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse),
            Description = "Invalid JSON supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, Json.MediaType, typeof(string),
            Description = "The URL to be used to start an external authorisation process")]
        [Function("GenerateAuthorisationUrl")]
        [ApiKeyAuthorize]
        public static async Task<HttpResponseData> GenerateAuthorisationUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/auth/{provider}/generate-url")]
            HttpRequestData req,
            FunctionContext executionContext,
            string provider)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            ICallbackService callbackService = executionContext.InstanceServices.GetService<ICallbackService>() ?? throw new NotImplementedException();

            try
            {
                IResponse response = callbackService.GenerateUrl(provider, req.Url.Query);

                return await req.CommonResponseHandler<AuthCallbackGeneratorResponse>(response);
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
                // Need to simplify to one service call like GenerateAuthorisationUrl function
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
        
        [OpenApiOperation("AuthorisationValidation", Summary = "Validate HMAC token")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path, Description = "The name of the provider, must be one of our supported providers")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse), Description = "Invalid JSON supplied")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "Details of the account that has been setup")]
        [Function("AuthorisationValidation")]
        public static HttpResponseData Validate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/auth/validate/{provider}")]
            HttpRequestData req,
            string provider,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            IHmacService hmacService = executionContext.InstanceServices.GetService<IHmacService>() ?? throw new NotImplementedException();
            IAuthorisationPreparationService authorisationPreparationService = executionContext.InstanceServices.GetService<IAuthorisationPreparationService>() ?? throw new NotImplementedException();
            
            try
            {
                bool supportedProvider = provider.TryParseIgnoreCase(out PosProviders parsedProvider);

                if (!supportedProvider)
                {
                    logger.Warning("{Provider} is an unsupported provider", provider);
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                List<KeyValuePair<string, string>> queryStrings = authorisationPreparationService.GetParameters(parsedProvider, req);
                
                bool isValid = hmacService.Validate(parsedProvider, queryStrings);

                if (!isValid)
                {
                    logger.Warning("There has been a problem validating the callback request");
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

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