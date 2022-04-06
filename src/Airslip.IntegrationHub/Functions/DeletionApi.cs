using Airslip.Common.Auth.Data;
using Airslip.Common.Auth.Functions.Attributes;
using Airslip.Common.Deletion.Interfaces;
using Airslip.Common.Deletion.Models;
using Airslip.Common.Functions.Interfaces;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using JetBrains.Annotations;
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

namespace Airslip.IntegrationHub.Functions;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class DeletionApi
{
    [OpenApiOperation(nameof(DeletionApi), Summary = "Deletes a store from middleware integration")]
    [OpenApiSecurity(AirslipSchemeOptions.ApiKeyScheme, SecuritySchemeType.ApiKey, Name = AirslipSchemeOptions.ApiKeyHeaderField, In = OpenApiSecurityLocationType.Header)]
    [OpenApiRequestBody(Json.MediaType, typeof(DeleteRequest))]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Api Key supplied")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse), Description = "Invalid JSON supplied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "Account was deleted successfully")]
    [OpenApiParameter("provider", In = ParameterLocation.Path, Required = true)]
    [OpenApiParameter("accountId", In = ParameterLocation.Path, Required = true)]
    [Function(nameof(DeletionApi))]
    [ApiKeyAuthorize]
    public static async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/delete/{provider}/{accountId}")]
        HttpRequestData req, 
        string provider,
        string accountId,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.InstanceServices.GetService<ILogger>()!;
        IDeletionService deletionService = executionContext.InstanceServices.GetService<IDeletionService>() ?? throw new NotImplementedException();
        IFunctionApiTools functionApiTools = executionContext.InstanceServices.GetService<IFunctionApiTools>() ?? throw new NotImplementedException();

        try
        {
            DeleteRequest deleteRequest = await req.Body.DeserializeStream<DeleteRequest>();
           
            IResponse response = await deletionService.DeleteRecord(provider, accountId, deleteRequest);

            return await functionApiTools.CommonResponseHandler<DeleteResponse>(req, response);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exception when deleting {Provider}", provider);
            HttpResponseData response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(ex.Message, HttpStatusCode.BadRequest);
            return response;
        }
    }
}