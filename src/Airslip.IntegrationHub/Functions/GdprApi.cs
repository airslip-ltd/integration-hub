using Airslip.Common.Auth.Data;
using Airslip.Common.Auth.Functions.Extensions;
using Airslip.Common.Functions.Interfaces;
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
    public static class GdprApi
    {
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
            IRequestValidationService validationService = executionContext.InstanceServices.GetService<IRequestValidationService>() ?? throw new NotImplementedException();
            IFunctionApiTools functionApiTools = executionContext.InstanceServices.GetService<IFunctionApiTools>() ?? throw new NotImplementedException();

            try
            {
                IResponse validationResponse = validationService.ValidateRequest(req, provider, AuthRequestTypes.Authorise);
                if (validationResponse is not ISuccess)
                    return await functionApiTools.CommonResponseHandler<ErrorResponse>(req, validationResponse);
                
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