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
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests.Billing;
using Airslip.IntegrationHub.Core.Requests.GDPR;
using Microsoft.Extensions.Options;

namespace Airslip.IntegrationHub.Functions
{
    public static class BillingApi
    {
        [OpenApiOperation("Billing", Summary = "Process Billing")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path, Description = "The name of the provider, must be one of our supported providers")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Identity supplied")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid JSON supplied")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        [Function("Billing")]
        public static async Task<HttpResponseData> Billing(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/billing/{provider}")]
            HttpRequestData req,
            string provider,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ?? throw new NotImplementedException();
            IRequestValidationService validationService = executionContext.InstanceServices.GetService<IRequestValidationService>() ?? throw new NotImplementedException();
            IProviderDiscoveryService providerDiscoveryService = executionContext.InstanceServices.GetService<IProviderDiscoveryService>() ?? throw new NotImplementedException();
            IBillingService billingService = executionContext.InstanceServices.GetService<IBillingService>() ?? throw new NotImplementedException();
            IFunctionApiTools functionApiTools = executionContext.InstanceServices.GetService<IFunctionApiTools>() ?? throw new NotImplementedException();

            try
            {
                ProviderDetails? providerDetails = providerDiscoveryService.GetProviderDetails(provider);

                if (providerDetails is null)
                {
                    logger.Warning("{Provider} is an unsupported provider", provider);
                    return await functionApiTools.Unauthorised(req,
                        new UnauthorisedResponse(provider, "Hmac validation failed for request"));
                }
                
                if (!validationService.ValidateRequest(providerDetails, req, AuthRequestTypes.Billing))
                {
                    logger.Information("Hmac validation failed for request");
                    return req.CreateResponse(HttpStatusCode.Unauthorized);
                }

                BillingRequest billingRequest = await req.Body.DeserializeStream<BillingRequest>();

                logger.Information("BillingRequest request made for {Shop}", billingRequest.Shop);

                IResponse authorisedResponse = await billingService.Create(billingRequest);
               
                return await functionApiTools.CommonResponseHandler<BillingResponse>(req, authorisedResponse);
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Unhandled error message {ErrorMessage}", e.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}