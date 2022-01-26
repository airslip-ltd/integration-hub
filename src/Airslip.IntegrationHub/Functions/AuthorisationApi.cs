using Airslip.Common.Auth.Data;
using Airslip.Common.Auth.Functions.Attributes;
using Airslip.Common.Auth.Functions.Extensions;
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
using Airslip.IntegrationHub.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Airslip.IntegrationHub.Functions
{
    public static class AuthorisationApi
    {
        [OpenApiOperation("GenerateAuthorisationUrl",
            Summary = "The generation of the URL to authorise an OAUTH application")]
        [OpenApiSecurity(AirslipSchemeOptions.ApiKeyScheme, SecuritySchemeType.ApiKey,
            Name = AirslipSchemeOptions.ApiKeyHeaderField, In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Invalid Api Key supplied")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path,
            Description = "The name of the provider, must be one of our supported providers")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse),
            Description = "Invalid JSON supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, Json.MediaType, typeof(string),
            Description = "The URL to be used to start an external authorisation process")]
        [Function("GenerateAuthorisationUrl")]
        [ApiKeyAuthorize]
        public static async Task<HttpResponseData> GenerateAuthUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/auth/{provider}/generate-url")]
            HttpRequestData req,
            FunctionContext executionContext,
            string provider)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ??
                             throw new NotImplementedException();
            IProviderDiscoveryService providerDiscoveryService =
                executionContext.InstanceServices.GetService<IProviderDiscoveryService>() ??
                throw new NotImplementedException();

            bool supportedProvider = provider.TryParseIgnoreCase(out PosProviders parsedProvider);

            if (!supportedProvider)
            {
                logger.Warning("Unsupported provider {Provider}", provider);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string callbackUrl = providerDiscoveryService.GenerateCallbackUrl(parsedProvider, req.Url.Query);

            return await req.CommonResponseHandler<AuthCallbackGeneratorResponse>(
                new AuthCallbackGeneratorResponse(callbackUrl));
        }

        [OpenApiOperation("AuthorisationCallback", Summary = "Callback to authorise a service with using OAUTH")]
        [OpenApiParameter("provider", Required = true, In = ParameterLocation.Path,
            Description = "The name of the provider, must be one of our supported providers")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, Json.MediaType, typeof(ErrorResponse),
            Description = "Invalid JSON supplied")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, Json.MediaType, typeof(AccountResponse),
            Description = "Details of the account that has been setup")]
        [Function("AuthorisationCallback")]
        public static async Task<HttpResponseData> Callback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "v1/auth/callback/{provider}")]
            HttpRequestData req,
            string provider,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.InstanceServices.GetService<ILogger>() ??
                             throw new NotImplementedException();
            IProviderDiscoveryService providerDiscoveryService =
                executionContext.InstanceServices.GetService<IProviderDiscoveryService>() ??
                throw new NotImplementedException();
            IntegrationMiddlewareClient httpClient =
                executionContext.InstanceServices.GetService<IntegrationMiddlewareClient>() ??
                throw new NotImplementedException();

            bool supportedProvider = provider.TryParseIgnoreCase(out PosProviders parsedProvider);

            if (!supportedProvider)
            {
                logger.Warning("Unsupported provider {Provider}", provider);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            //Validate HMAC another way, Using the strongly typed object
            List<KeyValuePair<string, string>> queryStrings = GetParameters(parsedProvider, req);

            bool isValid = providerDiscoveryService.ValidateHmac(parsedProvider, queryStrings);

            if (!isValid)
            {
                logger.Warning("There has been a problem validating the callback request");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IProviderAuthorisation providerAuthorisingDetail = GetProviderAuthorisationDetail(parsedProvider, req);

            IResponse authorisedResponse = await httpClient.SendToMiddleware(parsedProvider, providerAuthorisingDetail);

            return await req.CommonResponseHandler<AccountResponse>(authorisedResponse);
        }

        // Move to file on its own within this project
        private static IProviderAuthorisation GetProviderAuthorisationDetail(
            PosProviders provider,
            HttpRequestData req)
        {
            switch (provider)
            {
                case PosProviders.Shopify:
                    ShopifyAuthorisingDetail shopifyParams = req.Url.Query.GetQueryParams<ShopifyAuthorisingDetail>();
                    ShortLivedAuthorisationDetail shopifyShortLivedAuthDetail = shopifyParams;
                    shopifyShortLivedAuthDetail.FormatBaseUri(shopifyParams.Shop);
                    shopifyShortLivedAuthDetail.PermanentAccessUrl =
                        $"https://{shopifyParams.Shop}/admin/oauth/access_token";
                    //shopifyShortLivedAuthDetail.StoreName = shopifyParams.Shop.Replace(".myshopify.com", "");
                    return shopifyShortLivedAuthDetail;
                case PosProviders.Squarespace:
                    ShortLivedAuthorisationDetail squarespaceShortLivedAuthDetail =
                        req.Url.Query.GetQueryParams<SquarespaceAuthorisingDetail>();
                    // Get Base Uri
                    squarespaceShortLivedAuthDetail.PermanentAccessUrl =
                        "https://login.squarespace.com/api/1/login/oauth/provider/tokens";
                    return squarespaceShortLivedAuthDetail;
                case PosProviders.WoocommerceApi:
                    BasicAuthorisationDetail wooCommerceAuthDetail =
                        req.Body.DeserializeStream<WooCommerceAuthorisationDetail>();

                    return wooCommerceAuthDetail;
                default:
                    throw new NotImplementedException();
            }
        }

        private static List<KeyValuePair<string, string>> GetParameters(
            PosProviders provider,
            HttpRequestData req)
        {
            switch (provider)
            {
                case PosProviders.Shopify:
                case PosProviders.Squarespace:
                    return req.Url.Query.GetQueryParams().ToList();
                case PosProviders.WoocommerceApi:
                    WooCommerceAuthorisationDetail wooCommerceAuthorisationDetail = req.Body.DeserializeStream<WooCommerceAuthorisationDetail>();
                    string queryString = wooCommerceAuthorisationDetail.GetQueryString();
                    return  queryString.GetQueryParams(true).ToList();
                default:
                    throw new NotImplementedException();
            }
        }

        // Move to common
        public static T DeserializeStream<T>(this Stream requestBody) where T : class
        {
            StreamReader sr = new(requestBody);
            string payload = sr.ReadToEnd();
            return Json.Deserialize<T>(payload);
        }
        
        public static string GetQueryString(this object obj) {
            IEnumerable<string> properties = from p in obj.GetType().GetProperties()
                where p.GetValue(obj, null) != null
                select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return string.Join("&", properties.ToArray());
        }
    }
}