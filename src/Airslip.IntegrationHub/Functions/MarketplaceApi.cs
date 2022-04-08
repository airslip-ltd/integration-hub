using Airslip.Common.Auth.Data;
using Airslip.Common.Auth.Functions.Attributes;
using Airslip.Common.Deletion.Models;
using Airslip.Common.Functions.Interfaces;
using Airslip.Common.Types;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Requests;
using Airslip.IntegrationHub.Core.Requests.Marketplace;
using Airslip.IntegrationHub.Core.Responses;
using JetBrains.Annotations;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Functions;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class MarketplaceApi
{
    [OpenApiOperation("GenerateChallengeResponse", Summary = "Deletes a store from the third-party marketplace")]
    [OpenApiSecurity(AirslipSchemeOptions.ApiKeyScheme, SecuritySchemeType.ApiKey, Name = AirslipSchemeOptions.ApiKeyHeaderField, In = OpenApiSecurityLocationType.Header)]
    [OpenApiRequestBody(Json.MediaType, typeof(DeleteRequest))]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Api Key supplied")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse), Description = "Invalid JSON supplied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "App from the marketplace was deleted successfully")]
    [OpenApiParameter("provider", In = ParameterLocation.Path, Required = true)]
    [Function("GenerateChallengeResponse")]
    [ApiKeyAuthorize]
    public static async Task<HttpResponseData> GenerateChallengeResponse(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/delete/{provider}/marketplace")]
        HttpRequestData req,
        string provider,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.InstanceServices.GetService<ILogger>()!;
        IFunctionApiTools functionApiTools = executionContext.InstanceServices.GetService<IFunctionApiTools>() ?? throw new NotImplementedException();
        IOptions<PublicApiSettings> publicApiOptions = executionContext.InstanceServices.GetService<IOptions<PublicApiSettings>>() ?? throw new NotImplementedException();

        try
        {
            MarketplaceChallengeRequest request = req.Url.Query.GetQueryParams<MarketplaceChallengeRequest>();

            string verificationToken = "my-token-012345678901234567890123456789";
            
            PublicApiSetting callbackSettings = publicApiOptions.Value.GetSettingByName("Base");

            string endpoint = $"{callbackSettings.ToBaseUri()}/delete/{provider}/marketplace";

            IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            sha256.AppendData(Encoding.UTF8.GetBytes(request.ChallengeCode));
            sha256.AppendData(Encoding.UTF8.GetBytes(verificationToken));
            sha256.AppendData(Encoding.UTF8.GetBytes(endpoint));
            byte[] bytes = sha256.GetHashAndReset();
            string challengeResponse = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
            
            logger.Information("Logging challengeResponse: {ChallengeResponse}",  $"{request.ChallengeCode}|{verificationToken}|{endpoint}");

            return await functionApiTools.CommonResponseHandler<MarketplaceChallengeResponse>(req, new MarketplaceChallengeResponse(challengeResponse));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exception when deleting the marketplace for {Provider}", provider);
            HttpResponseData response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(ex.Message, HttpStatusCode.BadRequest);
            return response;
        }
    }

    [OpenApiOperation(nameof(MarketplaceApi), Summary = "Deletes a store from the third-party marketplace")]
    [OpenApiSecurity(AirslipSchemeOptions.ApiKeyScheme, SecuritySchemeType.ApiKey, Name = AirslipSchemeOptions.ApiKeyHeaderField, In = OpenApiSecurityLocationType.Header)]
    [OpenApiRequestBody(Json.MediaType, typeof(DeleteRequest))]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Api Key supplied")]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse), Description = "Invalid JSON supplied")]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "App from the marketplace was deleted successfully")]
    [OpenApiParameter("provider", In = ParameterLocation.Path, Required = true)]
    [Function(nameof(MarketplaceApi))]
    [ApiKeyAuthorize]
    public static async Task<HttpResponseData> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/delete/{provider}/marketplace")]
        HttpRequestData req, 
        string provider,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.InstanceServices.GetService<ILogger>()!;
        IMarketplaceService marketplaceService = executionContext.InstanceServices.GetService<IMarketplaceService>() ?? throw new NotImplementedException();
        IFunctionApiTools functionApiTools = executionContext.InstanceServices.GetService<IFunctionApiTools>() ?? throw new NotImplementedException();

        try
        {
            DeleteMarketplaceRequest request =await req.Body.DeserializeStream<DeleteMarketplaceRequest>();
           
            IResponse response = await marketplaceService.Delete(provider, request);
    
            return await functionApiTools.CommonResponseHandler<Success>(req, response);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exception when deleting the marketplace for {Provider}", provider);
            return await functionApiTools.CommonResponseHandler<Success>(req, Success.Instance);
        }
    }
}