using Airslip.Common.Security.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Models.AmazonSP;
using Airslip.IntegrationHub.Core.Models.BigCommerce;
using Airslip.IntegrationHub.Core.Models.eBay;
using Airslip.IntegrationHub.Core.Models.Ecwid;
using Airslip.IntegrationHub.Core.Models.Etsy;
using Airslip.IntegrationHub.Core.Models.Shopify;
using Airslip.IntegrationHub.Core.Models.Squarespace;
using Airslip.IntegrationHub.Core.Models.ThreeDCart;
using Airslip.IntegrationHub.Core.Models.WooCommerce;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Airslip.IntegrationHub.Core.Implementations;

public class AuthorisationPreparationService : IAuthorisationPreparationService
{
    private readonly EncryptionSettings _encryptionSettings;

    public AuthorisationPreparationService(IOptions<EncryptionSettings> encryptionOptions)
    {
        _encryptionSettings = encryptionOptions.Value;
    }

    public IProviderAuthorisation GetProviderAuthorisationDetail(
        ProviderDetails providerDetails,
        HttpRequestData req)
    {
        // Step 2: Add provider to get access token
        switch (providerDetails.Provider)
        {
            case PosProviders.Shopify:
                ShortLivedAuthorisationDetail s = req.Url.Query.GetQueryParams<ShopifyAuthorisingDetail>();
                s.FormatBaseUri(s.StoreName);
                s.PermanentAccessUrl = $"https://{s.StoreName}/admin/oauth/access_token";
                s.DecryptSensitiveInformation(_encryptionSettings.PassPhraseToken);
                return s;
            case PosProviders.Squarespace:
                ShortLivedAuthorisationDetail ss = req.Url.Query.GetQueryParams<SquarespaceAuthorisingDetail>();
                ss.PermanentAccessUrl = "https://login.squarespace.com/api/1/login/oauth/provider/tokens";
                ss.DecryptSensitiveInformation(_encryptionSettings.PassPhraseToken);
                return ss;
            case PosProviders.WoocommerceApi:
                BasicAuthorisationDetail w = req.Body.DeserializeFunctionStream<WooCommerceAuthorisationDetail>();
                w.DecryptSensitiveInformation(_encryptionSettings.PassPhraseToken);
                return w;
            case PosProviders.EBay:
                ShortLivedAuthorisationDetail e = req.Url.Query.GetQueryParams<EbayAuthorisingDetail>();
                e.PermanentAccessUrl = providerDetails.ProviderSetting.BaseUri + "/identity/v1/oauth2/token";
                e.DecryptSensitiveInformation(_encryptionSettings.PassPhraseToken);
                return e;
            case PosProviders.EtsyAPIv3:
                ShortLivedAuthorisationDetail et = req.Url.Query.GetQueryParams<EtsyAPIv3AuthorisingDetail>();
                et.DecryptSensitiveInformation(_encryptionSettings.PassPhraseToken);
                return et;
            case PosProviders.BigcommerceApi:
                ShortLivedAuthorisationDetail b = req.Url.Query.GetQueryParams<BigCommerceApiAuthorisingDetail>();
                b.PermanentAccessUrl = "https://login.bigcommerce.com/oauth2/token";
                providerDetails.ProviderSetting.Scope = b.Scope ?? string.Empty;
                b.DecryptSensitiveInformation(_encryptionSettings.PassPhraseToken);
                return b;
            case PosProviders._3DCart:
                ShortLivedAuthorisationDetail t = req.Url.Query.GetQueryParams<ThreeDCartAuthorisingDetail>();
                if(!string.IsNullOrEmpty(t.ErrorMessage))
                    return new ErrorAuthorisingDetail { ErrorMessage = t.ErrorMessage, ErrorCode = t.ErrorCode};
                
                t.PermanentAccessUrl = providerDetails.ProviderSetting.FormatBaseUri("apirest") + "/oauth/token";
                t.DecryptSensitiveInformation(_encryptionSettings.PassPhraseToken);
                return t;
            case PosProviders.Ecwid:
                EcwidAuthorisingDetail ec = req.Url.Query.GetQueryParams<EcwidAuthorisingDetail>();
                if(!string.IsNullOrEmpty(ec.ErrorMessage))
                    return new ErrorAuthorisingDetail { ErrorMessage = ec.ErrorMessage, ErrorCode = ec.ErrorCode};
                ec.PermanentAccessUrl = "https://my.ecwid.com/api/oauth/token";
                ec.DecryptSensitiveInformation(_encryptionSettings.PassPhraseToken);
                return ec;
            case PosProviders.AmazonSP: 
                ShortLivedAuthorisationDetail a = req.Url.Query.GetQueryParams<AmazonSPAuthorisingDetail>();
                a.PermanentAccessUrl = "https://api.amazon.com/auth/o2/token";
                a.DecryptSensitiveInformation(_encryptionSettings.PassPhraseToken);
                return a;
            default:
                throw new NotImplementedException();
        }
    }

    public List<KeyValuePair<string, string>> GetParameters(
        ProviderDetails providerDetails,
        HttpRequestData req)
    {
        // Step 3: If using POST then add custom logic here
        switch (providerDetails.Provider)
        {
            case PosProviders.WoocommerceApi:
                WooCommerceAuthorisationDetail wooCommerceAuthorisationDetail = req.Body.DeserializeFunctionStream<WooCommerceAuthorisationDetail>();
                string queryString = wooCommerceAuthorisationDetail.GetQueryString();
                return queryString.GetQueryParams(true).ToList();
            default:
                return req.Url.Query.GetQueryParams().ToList();
        }
    }
}