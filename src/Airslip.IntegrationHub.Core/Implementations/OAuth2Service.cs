using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Failures;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Models.BigCommerce;
using Airslip.IntegrationHub.Core.Models.eBay;
using Airslip.IntegrationHub.Core.Models.Ecwid;
using Airslip.IntegrationHub.Core.Models.Etsy;
using Airslip.IntegrationHub.Core.Models.Squarespace;
using Airslip.IntegrationHub.Core.Models.ThreeDCart;
using Serilog;
using System;
using System.Collections.Generic;
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
                _logger.Error(
                    "Error posting request to provider for Url {PostUrl}, response code: {StatusCode}, Error: {ErrorResponse}",
                    httpRequestMessage.RequestUri,
                    response.StatusCode,
                    content);

                // Parse and return proper error
                return new HandledError(nameof(ExchangeCodeForAccessToken), "Error exchanging code for access token");
            }
            
            Dictionary<string, string> parameters = Json.Deserialize<Dictionary<string, string>>(content);

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
    
    public HttpRequestMessage GetHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail)
    {
        return providerDetails.Provider switch
        {
            PosProviders.Squarespace => new SquarespacePermanentAccessHttpRequestMessage(
                providerDetails,
                shortLivedAuthorisationDetail),
            PosProviders.EtsyAPIv3 => new EtsyAPIv3PermanentAccessHttpRequestMessage(
                providerDetails,
                shortLivedAuthorisationDetail),
            PosProviders.EBay => new EbayPermanentAccessHttpRequestMessage(
                providerDetails,
                shortLivedAuthorisationDetail),
            PosProviders.BigcommerceApi => new BigCommerceApiPermanentAccessHttpRequestMessage(
                providerDetails,
                shortLivedAuthorisationDetail),
            PosProviders._3DCart => new ThreeDCartPermanentAccessHttpRequestMessage(
                providerDetails,
                shortLivedAuthorisationDetail),
            PosProviders.Ecwid => new EcwidPermanentAccessHttpRequestMessage(
                providerDetails,
                shortLivedAuthorisationDetail),
            _ => throw new NotImplementedException()
        };
    }

    public BasicAuthorisationDetail ParseResponseMessage(
        string content,
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail)
    {
        BasicAuthorisationDetail basicAuth = new();

        // Step 5: Add case for permanent access token
        switch (providerDetails.Provider)
        {
            case PosProviders.Squarespace:
                basicAuth = Json.Deserialize<SquarespaceAuthorisationDetail>(content);
                break;
            case PosProviders.EBay:
                basicAuth = Json.Deserialize<EbayAuthorisationDetail>(content);
                break;
            case PosProviders.EtsyAPIv3:
                basicAuth = Json.Deserialize<EtsyAPIv3AuthorisationDetail>(content);
                break;
            case PosProviders.BigcommerceApi:
                basicAuth = Json.Deserialize<BigCommerceApiAuthorisationDetail>(content);
                break;
            case PosProviders._3DCart:
                basicAuth = Json.Deserialize<ThreeDCartAuthorisationDetail>(content);
                break;
            case PosProviders.Ecwid:
                basicAuth = Json.Deserialize<EcwidAuthorisationDetail>(content);
                break;
        }
        
        return basicAuth;
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
