using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using System.Net.Http;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using Airslip.IntegrationHub.Core.Responses;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub
{
    public class IntegrationMiddlewareClient
    {
        private readonly IProviderDiscoveryService _providerDiscoveryService;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public IntegrationMiddlewareClient(
            IProviderDiscoveryService providerDiscoveryService,
            HttpClient httpClient,
            ILogger logger)
        {
            _providerDiscoveryService = providerDiscoveryService;
            _httpClient = httpClient;
            _logger = logger;
        }
        
        public async Task<IResponse> SendToMiddleware(PosProviders provider, IProviderAuthorisation providerAuthorisingDetail)
        {
            ProviderDetails providerDetails = _providerDiscoveryService.GetProviderDetails(provider);

            MiddlewareAuthorisationRequest middlewareAuthorisationBody = providerDetails.ProviderSetting.AuthStrategy switch
            {
                ProviderAuthStrategy.ShortLived => await _providerDiscoveryService.QueryPermanentAccessToken(providerDetails, (ShortLivedAuthorisationDetail)providerAuthorisingDetail),
                ProviderAuthStrategy.Basic =>  _providerDiscoveryService.GetMiddlewareAuthorisation(provider, (BasicAuthorisationDetail)providerAuthorisingDetail),
                ProviderAuthStrategy.Bridge => _providerDiscoveryService.GetMiddlewareAuthorisation(provider,(BasicAuthorisationDetail)providerAuthorisingDetail),
                _ => throw new NotSupportedException()
            };

            string url = Endpoints.Result(providerDetails.DestinationBaseUri, provider);

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
                        Json.Serialize(middlewareAuthorisationBody),
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
                    
                    throw new Exception("Error exchanging short lived token in the providers middleware");
                }
                
                _logger.Information("Got response for post to integration middleware for Url {PostUrl}, response code: {StatusCode}",
                    url, response.StatusCode);

                return await response.CommonResponseHandler<AccountResponse>();
            }
            catch (HttpRequestException hre)
            {
                _logger.Error(hre,
                    "Error posting request to integration middleware for Url {PostUrl}, response code: {StatusCode}",
                    url, hre.StatusCode);
                return new InvalidResource("", "Fail");
            }
            catch (Exception ee)
            {
                _logger.Fatal(ee, "Error posting request to integration middleware for Url {PostUrl}", url);
                return new InvalidResource("", "Fail");
            }
        }
    }

    internal static class Endpoints
    {
        public static string Result(string baseUri, PosProviders provider) =>
            $"{baseUri}/auth/{provider}";
    }
}