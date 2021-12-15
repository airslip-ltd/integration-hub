using Airslip.Common.Auth.Data;
using Airslip.Common.Auth.Functions.Extensions;
using Airslip.Common.Auth.Functions.Interfaces;
using Airslip.Common.Auth.Models;
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

namespace Airslip.IntegrationHub.Functions
{
    public static class AuthorisationApi
    {
        [OpenApiOperation("GenerateAuthorisationUrl",
            Summary = "The generation of the URL to authorise an OAUTH application")]
        [OpenApiSecurity(AirslipSchemeOptions.ApiKeyScheme, SecuritySchemeType.ApiKey,
            Name = AirslipSchemeOptions.ApiKeyHeaderField, In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Api Key supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse),
            Description = "Invalid JSON supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, Json.MediaType, typeof(string),
            Description = "The URL to be used to start an external authorisation process")]
        [Function("GenerateAuthorisationUrl")]
        public static async Task<HttpResponseData> GenerateAuthUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/auth/{provider}/generate-url")]
            HttpRequestData req,
            FunctionContext executionContext,
            string provider,
            string? shopName = null,
            bool? isOnline = null)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            IApiRequestAuthService authService = executionContext.InstanceServices.GetService<IApiRequestAuthService>() ?? throw new NotImplementedException();
            IProviderDiscoveryService providerDiscoveryService = executionContext.InstanceServices.GetService<IProviderDiscoveryService>() ?? throw new NotImplementedException();
            ICustomerPortalClient customerPortalClient = executionContext.InstanceServices.GetService<ICustomerPortalClient>() ?? throw new NotImplementedException();
            KeyAuthenticationResult authenticationResult = await authService.Handle(req);
            
            logger.Information("Testing");

            if (authenticationResult.AuthResult != AuthResult.Success)
            {
                logger.Error("Authorisation unsuccessful {ErrorMessage}", authenticationResult.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            bool supportedProvider = Enum.TryParse(provider, out PosProviders parsedProvider);

            if (!supportedProvider)
            {
                logger.Warning("Unsupported provider {Provider}", provider);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            // TODO: Implement client to get newly added account id
            IResponse response = await customerPortalClient.CreateStub();

            switch (response)
            {
                case AccountResponse account:

                    string callbackUrl = providerDiscoveryService.GenerateCallbackUrl(parsedProvider, account.Id, shopName, isOnline);

                    return await req.CommonResponseHandler<AuthCallbackGeneratorResponse>(
                        new AuthCallbackGeneratorResponse(callbackUrl));
                
                default: 
                    return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [OpenApiOperation("AuthorisationCallback", Summary = "Callback to authorise a service with using OAUTH")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse),
            Description = "Invalid JSON supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, Json.MediaType, typeof(AuthorisationResponse),
            Description = "Details of the account that has been setup")]
        [Function("AuthorisationCallback")]
        public static async Task<HttpResponseData> Callback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/auth/{provider}/callback")]
            HttpRequestData req,
            string provider,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ??
                             throw new NotImplementedException();
            IntegrationMiddlewareClient httpClient =
                executionContext.InstanceServices.GetService<IntegrationMiddlewareClient>() ??
                throw new NotImplementedException();

            IResponse authorisedResponse = await httpClient.SendToMiddleware(provider, req);

            return await req.CommonResponseHandler<ISuccess>(authorisedResponse);
        }
    }
}