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
using System.Collections.Generic;
using System.Linq;

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
        [ApiKeyAuthorize]
        public static async Task<HttpResponseData> GenerateAuthUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/auth/{provider}/generate-url")]
            HttpRequestData req,
            FunctionContext executionContext,
            string provider)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            IProviderDiscoveryService providerDiscoveryService = executionContext.InstanceServices
                .GetService<IProviderDiscoveryService>() ?? throw new NotImplementedException();
            
            bool supportedProvider = Enum.TryParse(provider, true, out PosProviders parsedProvider);

            if (!supportedProvider)
            {
                logger.Warning("Unsupported provider {Provider}", provider);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
          
            string callbackUrl = providerDiscoveryService.GenerateCallbackUrl(parsedProvider, req.Url.Query);

            return await req.CommonResponseHandler<AuthCallbackGeneratorResponse>(
                new AuthCallbackGeneratorResponse(callbackUrl));
        }

        [OpenApiOperation("AuthorisationCallback", Summary = "Callback to authorise a service with using OAUTH")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path, Description = "The name of the provider, must be one of our supported providers")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse), Description = "Invalid JSON supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, Json.MediaType, typeof(AccountResponse), Description = "Details of the account that has been setup")]
        [Function("AuthorisationCallback")]
        public static async Task<HttpResponseData> Callback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/auth/callback/{provider}")]
            HttpRequestData req,
            string provider,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            IProviderDiscoveryService providerDiscoveryService = executionContext.InstanceServices.GetService<IProviderDiscoveryService>() ?? throw new NotImplementedException();
            IntegrationMiddlewareClient httpClient = executionContext.InstanceServices.GetService<IntegrationMiddlewareClient>() ?? throw new NotImplementedException();

            bool supportedProvider = Enum.TryParse(provider, true, out PosProviders parsedProvider);

            if (!supportedProvider)
            {
                logger.Warning("Unsupported provider {Provider}", provider);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            
            List<KeyValuePair<string, string>> queryStrings = req.Url.Query.GetQueryParams().ToList();

            bool isValid = providerDiscoveryService.Validate(parsedProvider, queryStrings);
            
            if (!isValid)
            {
                logger.Warning("There has been a problem validating the callback request");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IResponse authorisedResponse = await httpClient.SendToMiddleware(provider, req.Url.Query);

            return await req.CommonResponseHandler<AccountResponse>(authorisedResponse);
        }
    }
}