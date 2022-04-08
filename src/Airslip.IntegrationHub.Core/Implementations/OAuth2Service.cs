using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class OAuth2Service : IOAuth2Service
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public OAuth2Service(
        IHttpClientFactory httpClientFactory,
        ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IResponse> ExchangeCodeForAccessToken(string provider, HttpRequestMessage httpRequestMessage)
    {
        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Add error column name object, parse then log proper error
                _logger.Error(
                    "Error posting request to provider for Url {PostUrl}, response code: {StatusCode}, Error: {ErrorResponse}",
                    httpRequestMessage.RequestUri,
                    response.StatusCode,
                    content);

                return new HandledError(nameof(ExchangeCodeForAccessToken), "Error exchanging code for access token");
            }

            Dictionary<string, object> parsedParameters = Json.Deserialize<Dictionary<string, object>>(content);

            Dictionary<string, string> parameters = parsedParameters
                .ToDictionary(k => k.Key, k => k.Value.ToString()!);

            return new AccessTokenModel(parameters);
        }
        catch (HttpRequestException hre)
        {
            _logger.Error(hre,
                "Error posting request to OAuth2 request for provider {Provider}, response code: {StatusCode}",
                provider,
                hre.StatusCode);

            throw;
        }
        catch (Exception ee)
        {
            _logger.Fatal(ee,
                "Unhandled error posting request to OAuth2 endpoint for provider {Provider}",
                provider);
            throw;
        }
    }
}

public class AccessTokenModel : ISuccess
{
    public Dictionary<string, string> Parameters { get; }

    public AccessTokenModel(Dictionary<string, string> parameters)
    {
        Parameters = parameters;
    }
}
