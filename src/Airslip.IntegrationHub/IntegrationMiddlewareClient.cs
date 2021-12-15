using Airslip.Common.Types;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net.Http;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Requests;
using Serilog;
using System;
using System.Net;
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

        public async Task<IResponse> SendToMiddleware(string provider, HttpRequestData requestData)
        {
            ProviderDetails providerDetails = _providerDiscoveryService.GetProviderDetails(provider);
            
            string url = Endpoints.Result(providerDetails.DestinationBaseUri, provider);
            
            try
            {
                // TODO: Create switch for different middleware providers
                HttpRequestMessage httpRequestMessage = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Headers =
                    {
                        { "x-api-key", providerDetails.PublicApiSetting.ApiKey}
                    },
                    Content = new StringContent(
                        Json.Serialize(new VendAuthorisationCallBackRequest()),
                        Encoding.UTF8,
                        Json.MediaType)
                };

                HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.Error("Error sending request to integration middleware for Url {Url}, response code: {StatusCode}", url, response.StatusCode);
                }

                return Success.Instance;
            }
            catch (HttpRequestException hre)
            {
                _logger.Error(hre, "Error posting request to integration middleware for Url {PostUrl}, response code: {StatusCode}", url, hre.StatusCode);
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
        public static string Result(string baseUri, string provider) =>
            $"{baseUri}/auth/{provider}";
    }
}