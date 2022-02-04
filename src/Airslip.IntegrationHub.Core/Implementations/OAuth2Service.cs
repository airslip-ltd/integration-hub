using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Models.Ecwid;
using Airslip.IntegrationHub.Core.Models.Shopify;
using Airslip.IntegrationHub.Core.Models.Squarespace;
using Airslip.IntegrationHub.Core.Models.ThreeDCart;
using Airslip.IntegrationHub.Core.Requests;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations;

public class OAuth2Service : IOAuth2Service
{
    private readonly IInternalMiddlewareService _internalMiddlewareService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public OAuth2Service(
        IInternalMiddlewareService internalMiddlewareService, 
        IHttpClientFactory httpClientFactory,
        ILogger logger)
    {
        _internalMiddlewareService = internalMiddlewareService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<MiddlewareAuthorisationRequest> QueryPermanentAccessToken(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail)
    {
        try
        {
            // Change create client name
            HttpClient httpClient = _httpClientFactory.CreateClient(providerDetails.Provider.ToString());

            HttpRequestMessage httpRequestMessage = GetHttpRequestMessage(
                providerDetails,
                shortLivedAuthorisationDetail);

            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error(
                    "Error posting request to provider for Url {PostUrl}, response code: {StatusCode}, Error: {ErrorResponse}",
                    shortLivedAuthorisationDetail.PermanentAccessUrl,
                    response.StatusCode,
                    content);

                return new MiddlewareAuthorisationRequest();
            }

            // 101 invalid request
            BasicAuthorisationDetail basicAuth = ParseResponseMessage(
                content,
                providerDetails,
                shortLivedAuthorisationDetail);

            return _internalMiddlewareService.BuildMiddlewareAuthorisationModel(
                providerDetails,
                basicAuth);
        }
        catch (HttpRequestException hre)
        {
            _logger.Error(hre,
                "Error posting request to OAuth2 request for provider {Provier}, response code: {StatusCode}",
                providerDetails.Provider, hre.StatusCode);
            
            throw;
        }
        catch (Exception ee)
        {
            _logger.Fatal(ee, "Unhandled error posting request to OAuth2 endpoint for provider {Provider}", providerDetails.Provider);
            throw;
        }
    }

    // Step X: Add provider for OAuth2
    public HttpRequestMessage GetHttpRequestMessage(
        ProviderDetails providerDetails,
        ShortLivedAuthorisationDetail shortLivedAuthorisationDetail)
    {
        return providerDetails.Provider switch
        {
            PosProviders.Shopify => new ShopifyPermanentAccessHttpRequestMessage(
                providerDetails,
                shortLivedAuthorisationDetail),
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
            case PosProviders.Shopify:
                basicAuth = Json.Deserialize<ShopifyAuthorisationDetail>(content);
                basicAuth.Login = providerDetails.ProviderSetting.ApiSecret;
                basicAuth.Shop = shortLivedAuthorisationDetail.StoreName;
                break;
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
#if DEBUG
                basicAuth.EncryptedUserInfo =
                    "+bvlByiyWY0En780uwUgFO7VVvlUF6/NeBawhojg1U9vxfESckFkeWAkk4LkK/qNKNGJDybhMoGa5ZKYU/glEthNo7pCebSnNG12Ncir4jpUTyNheMDeNSAXbLLhWj+SiOOgmWaektxAX4MBZwwIZGNn2dEZfBDshoSta3AODXX11EVTrgOEGpyt4n/SMUTCvD2WCAqDro5AUvQxtGnHy1MpGtdBVJ+yCA5dZJbqHpTWt629s7MfPp5Ey//AbIfA";
#endif
                break;
            case PosProviders._3DCart:
                basicAuth = Json.Deserialize<ThreeDCartAuthorisationDetail>(content);
                break;
            case PosProviders.Ecwid:
                basicAuth = Json.Deserialize<EcwidAuthorisationDetail>(content);
#if DEBUG
                basicAuth.EncryptedUserInfo =
                    "rBpyBX0TMNt1pGMBlJd2tchvSH/LQJkdjLsnUzMZCEC8KFkC+qBcb2Z3HBzhCiuFlGLaU3yeCDTiNrILCiQ+qOCdPvzt4+qGlmio177fpV0C+6egY0/wufaBfcIDYHCRGDWAbmDpiR8fKlJqP5kMuMr4jCCQ2XLfUwpmn3uAxR9Q7OVi2sBfEeBu3EQa7GRuHrNdB+wBagD9imdEPH9uC+Iw0PsuW4EL7ebOoNpU3VKc6d9uSfr86D5e+BgArPC8";
#endif
                break;
        }
        
        if( basicAuth.EncryptedUserInfo == string.Empty)
            basicAuth.EncryptedUserInfo = shortLivedAuthorisationDetail.EncryptedUserInfo;

        return basicAuth;
    }
}