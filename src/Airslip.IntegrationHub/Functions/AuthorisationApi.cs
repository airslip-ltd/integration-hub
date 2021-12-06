using Airslip.Common.Auth.Data;
using Airslip.Common.Auth.Functions.Extensions;
using Airslip.Common.Auth.Functions.Interfaces;
using Airslip.Common.Auth.Models;
using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Failures;
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
        [OpenApiOperation("GenerateAuthUrl", Summary = "The generation of the URL to authorise an OAUTH application")]
        [OpenApiSecurity(AirslipSchemeOptions.ApiKeyScheme, SecuritySchemeType.ApiKey, Name = AirslipSchemeOptions.ApiKeyHeaderField, In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Api Key supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse), Description = "Invalid JSON supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, Json.MediaType, typeof(string), Description = "The URL to be used to start an external authorisation process")]
        [Function("GenerateAuthorisationUrl")]
        public static async Task<HttpResponseData> GenerateAuthUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/auth/generate-url")]
            HttpRequestData req,
            string provider,
            string accountId,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            IApiRequestAuthService authService = executionContext.InstanceServices.GetService<IApiRequestAuthService>() ?? throw new NotImplementedException();
            IExternalAuthService externalAuthService = executionContext.InstanceServices.GetService<IExternalAuthService>() ?? throw new NotImplementedException();
            
            KeyAuthenticationResult authenticationResult = await authService.Handle(req);
            
            if (authenticationResult.AuthResult != AuthResult.Success)
            {
                logger.Error("Authorisation unsuccessful {ErrorMessage}", authenticationResult.Message);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
            }

            bool supportedProvider = Enum.TryParse(provider, out PosProviders parsedProvider);

            if(!supportedProvider)
            {
                logger.Warning("Unsupported provider {Provider}", provider);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string callbackUrl = externalAuthService.GenerateCallbackUrl(parsedProvider, accountId);

            return await req.CommonResponseHandler<AuthCallbackGeneratorResponse>(
                new AuthCallbackGeneratorResponse(callbackUrl));
        }
    }
}