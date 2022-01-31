using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Airslip.IntegrationHub.Services;

public class AuthorisationPreparationService : IAuthorisationPreparationService
{
    public IProviderAuthorisation GetProviderAuthorisationDetail(
        ProviderDetails providerDetails,
        HttpRequestData req)
    {
        // Step 2: Add provider to get access token
        switch (providerDetails.Provider)
        {
            case PosProviders.Shopify:
                ShortLivedAuthorisationDetail shopifyShortLivedAuthDetail =
                    req.Url.Query.GetQueryParams<ShopifyAuthorisingDetail>();
                shopifyShortLivedAuthDetail.FormatBaseUri(shopifyShortLivedAuthDetail.StoreName);
                shopifyShortLivedAuthDetail.PermanentAccessUrl =
                    $"https://{shopifyShortLivedAuthDetail.StoreName}/admin/oauth/access_token";
                return shopifyShortLivedAuthDetail;
            case PosProviders.Squarespace:
                ShortLivedAuthorisationDetail squarespaceShortLivedAuthDetail =
                    req.Url.Query.GetQueryParams<SquarespaceAuthorisingDetail>();
                squarespaceShortLivedAuthDetail.PermanentAccessUrl =
                    "https://login.squarespace.com/api/1/login/oauth/provider/tokens";
                return squarespaceShortLivedAuthDetail;
            case PosProviders.WoocommerceApi:
                BasicAuthorisationDetail wooCommerceAuthDetail =
                    req.Body.DeserializeFunctionStream<WooCommerceAuthorisationDetail>();
                return wooCommerceAuthDetail;
            case PosProviders.EBay:
                ShortLivedAuthorisationDetail ebayShortLivedAuthDetail =
                    req.Url.Query.GetQueryParams<EbayAuthorisingDetail>();
                ebayShortLivedAuthDetail.PermanentAccessUrl =
                    providerDetails.ProviderSetting.BaseUri + "/identity/v1/oauth2/token";
                return ebayShortLivedAuthDetail;
            case PosProviders.EtsyAPIv3:
                ShortLivedAuthorisationDetail etsyAuth = req.Url.Query.GetQueryParams<EtsyAPIv3AuthorisingDetail>();
                return etsyAuth;
            case PosProviders.BigcommerceApi:
                BigCommerceApiAuthorisingDetail bigCommerceAuth =
                    req.Url.Query.GetQueryParams<BigCommerceApiAuthorisingDetail>();
                bigCommerceAuth.PermanentAccessUrl = "https://login.bigcommerce.com/oauth2/token";
                providerDetails.ProviderSetting.Scope = bigCommerceAuth.Scope;
                return bigCommerceAuth;
            default:
                throw new NotImplementedException();
        }
    }

    public List<KeyValuePair<string, string>> GetParameters(
        PosProviders provider,
        HttpRequestData req)
    {
        // Step 3: If using POST then add custom logic here
        switch (provider)
        {
            case PosProviders.WoocommerceApi:
                WooCommerceAuthorisationDetail wooCommerceAuthorisationDetail =
                    req.Body.DeserializeFunctionStream<WooCommerceAuthorisationDetail>();
                string queryString = wooCommerceAuthorisationDetail.GetQueryString();
                return queryString.GetQueryParams(true).ToList();
            default:
                return req.Url.Query.GetQueryParams().ToList();
        }
    }
}