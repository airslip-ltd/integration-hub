using Airslip.Common.Security.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using Airslip.IntegrationHub.Core.Responses;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations
{
    public class InternalMiddlewareClient : IInternalMiddlewareClient
    {
        private readonly HttpClient _httpClient;
        private readonly EncryptionSettings _encryptionSettings;
        private readonly ILogger _logger;

        public InternalMiddlewareClient(
            HttpClient httpClient,
            IOptions<EncryptionSettings> encryptionOptions,
            ILogger logger)
        {
            _httpClient = httpClient;
            _encryptionSettings = encryptionOptions.Value;
            _logger = logger;
        }
        
        public async Task<IResponse> SendToMiddleware(
            ProviderDetails providerDetails,  
            MiddlewareAuthorisationRequest middlewareAuthorisationRequest)
        {
            string url = Endpoints.Result(providerDetails.MiddlewareDestinationBaseUri, providerDetails.Provider);

            try
            {
                _logger.Information("Posting to integration middleware for Url {PostUrl}",
                    url);
                
                HttpRequestMessage httpRequestMessage = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Headers =
                    {
                        { "x-api-key", providerDetails.PublicApiSetting.ApiKey}
                    },
                    Content = new StringContent(
                        Json.Serialize(middlewareAuthorisationRequest),
                        Encoding.UTF8,
                        Json.MediaType)
                };

                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Error(
                        "Error posting request to provider for Url {PostUrl}, response code: {StatusCode}", 
                        url, 
                        response.StatusCode);

                    string content = await response.Content.ReadAsStringAsync();
                    
                    try
                    {
                        return Json.Deserialize<ErrorResponses>(content);
                    }
                    catch (Exception)
                    {
                        return new ErrorResponse("MIDDLEWARE_ERROR", content);
                    }
                }
                
                _logger.Information("Got response for post to integration middleware for Url {PostUrl}, response code: {StatusCode}", url, response.StatusCode);

                return await response.CommonResponseHandler<AccountResponse>();
            }
            catch (HttpRequestException hre)
            {
                _logger.Error(hre,
                    "Error posting request to integration middleware for Url {PostUrl}, response code: {StatusCode}",
                    url, hre.StatusCode);
                return new InvalidResource(nameof(SendToMiddleware), hre.Message);
            }
            catch (Exception ee)
            {
                _logger.Fatal(ee, "Error posting request to integration middleware for Url {PostUrl}", url);
                return new InvalidResource("UNHANDLED_ERROR", ee.Message);
            }
        }
        
        public MiddlewareAuthorisationRequest BuildMiddlewareAuthorisationModel(
            ProviderDetails providerDetails,
            BasicAuthorisationDetail basicAuthorisationDetail)
        {
            SensitiveCallbackInfo sensitiveCallbackInfo = SensitiveCallbackInfo.DecryptCallbackInfo(
                basicAuthorisationDetail.EncryptedUserInfo,
                _encryptionSettings.PassPhraseToken);

            return new MiddlewareAuthorisationRequest
            {
                Provider = providerDetails.Provider.ToString(),
                StoreName = sensitiveCallbackInfo.Shop, // May need to consolidate store name and store url
                StoreUrl = providerDetails.ProviderSetting.FormatBaseUri(sensitiveCallbackInfo.Shop), // Need to change to StoreUrl
                Login = basicAuthorisationDetail.Login,
                Password = basicAuthorisationDetail.Password,
                EntityId = sensitiveCallbackInfo.EntityId,
                UserId = sensitiveCallbackInfo.UserId,
                AirslipUserType = sensitiveCallbackInfo.AirslipUserType,
                Environment = providerDetails.ProviderSetting.Environment,
                LocationId = providerDetails.ProviderSetting.LocationId,
            };
        }
    }

    internal static class Endpoints
    {
        public static string Result(string baseUri, PosProviders provider) =>
            $"{baseUri}/auth/{provider}";
    }
}