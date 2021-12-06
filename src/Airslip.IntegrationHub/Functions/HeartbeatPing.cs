using Airslip.Common.Monitoring.Interfaces;
using Airslip.Common.Monitoring.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Net;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Functions
{
    public static class HeartbeatPing
    {
        [OpenApiOperation(nameof(HeartbeatPing), Summary = "Ping operation to ensure service is contactable")]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        [Function(nameof(HeartbeatPing))]
        public static HttpResponseData Ping(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/heartbeat/ping")] HttpRequestData req,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>()!;
            logger.Information("Triggered HeartbeatPing");

            return req.CreateResponse(HttpStatusCode.OK);
        }

        [Function("HeartbeatHealth")]
        public static async Task<HttpResponseData> Health(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/heartbeat/health")] HttpRequestData req,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>()!;
            IHealthCheckService healthCheckService =
                executionContext.InstanceServices.GetService<IHealthCheckService>()!;

            logger.Information("Triggered HeartbeatHealth");
            HealthCheckResponse heartbeatResponse = await healthCheckService.CheckServices();

            HttpResponseData response = req
                .CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(heartbeatResponse);
            return response;
        }
    }
}