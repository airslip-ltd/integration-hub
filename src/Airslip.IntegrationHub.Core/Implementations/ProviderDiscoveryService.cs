using Airslip.Common.Security.Configuration;
using Airslip.Common.Security.Implementations;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Airslip.IntegrationHub.Core.Implementations
{
    public class ProviderDiscoveryService : IProviderDiscoveryService
    {
        private readonly SettingCollection<ProviderSetting> _providerSettings;
        private readonly PublicApiSettings _publicApiSettings;
        private readonly IOptions<EncryptionSettings> _encryptionOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public ProviderDiscoveryService(
            IOptions<SettingCollection<ProviderSetting>> providerOptions,
            IOptions<PublicApiSettings> publicApiOptions,
            IOptions<EncryptionSettings> encryptionOptions,
            IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            _encryptionOptions = encryptionOptions;
            _providerSettings = providerOptions.Value;
            _publicApiSettings = publicApiOptions.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public ProviderDetails GetProviderDetails(PosProviders provider)
        {
            ProviderSetting providerSetting = GetProviderSettings(provider);

            string internalMiddlewareName = GetInternalMiddlewareName(provider);

            PublicApiSetting publicApiSetting = _publicApiSettings.GetSettingByName(internalMiddlewareName);
            string destinationBaseUri = publicApiSetting.ToBaseUri();
            string redirectUri = $"{destinationBaseUri}/auth/callback/{provider}";

            return new ProviderDetails(
                provider,
                destinationBaseUri,
                redirectUri,
                publicApiSetting,
                providerSetting);
        }

        public ProviderSetting GetProviderSettings(PosProviders provider)
        {
            return _providerSettings.GetSettingByName(provider.ToString());
        }

        public string GenerateCallbackUrl(PosProviders provider, string queryString)
        {
            (string cipherUrl, SensitiveCallbackInfo generateCallbackAuthRequest) = GetEncryptedUserInformation(queryString);
            
            string destinationBaseUri = _publicApiSettings.GetSettingByName("Base").ToBaseUri();
            string redirectUri = generateCallbackAuthRequest.CallbackUrl ?? 
                                 $"{destinationBaseUri}/auth/callback/{provider}";
            
            ProviderSetting providerSetting = GetProviderSettings(provider);

            switch (provider)
            {
                case PosProviders.Vend:
                    return
                        $"{providerSetting.BaseUri}?response_type=code&client_id={providerSetting.AppId}&redirect_uri={redirectUri}&state={cipherUrl}";
                case PosProviders.SwanRetailMidas:
                    return string.Empty;
                case PosProviders.Volusion:
                    return string.Empty;
                case PosProviders.Shopify:
                    ShopifyProvider auth = queryString.GetQueryParams<ShopifyProvider>();
                    string grantOptions = auth.IsOnline ? "per-user" : "value";
                    return
                        $"{string.Format(providerSetting.BaseUri, auth.Shop)}/admin/oauth/authorize?client_id={providerSetting.AppId}&scope={providerSetting.Scope}&redirect_uri={redirectUri}&state={cipherUrl}&grant_options[]={grantOptions}";
                case PosProviders.Stripe:
                case PosProviders.SumUp:
                case PosProviders.IZettle:
                case PosProviders.EposNow:
                case PosProviders.WoocommerceApi:
                    WooCommerceProvider wooCommerce = queryString.GetQueryParams<WooCommerceProvider>();
                    return
                        $"{wooCommerce.Shop}/wc-auth/v1/authorize?app_name=Airslip&scope={providerSetting.Scope}&user_id={cipherUrl}&return_url=https://google.com&callback_url={redirectUri}";
                case PosProviders.Squarespace:
                    SquarespaceProvider squarespaceAuth = queryString.GetQueryParams<SquarespaceProvider>();
                    return
                        $"{string.Format(providerSetting.BaseUri, squarespaceAuth.Shop)}/api/1/login/oauth/provider/authorize?client_id={providerSetting.AppId}&scope={providerSetting.Scope}&redirect_uri={redirectUri}&state={cipherUrl}&access_type=offline";
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, "Not yet supported");
            }
        }
        
        public bool ValidateHmac(
            PosProviders provider,
            List<KeyValuePair<string, string>> queryStrings)
        {
            string? hmacKey = GetHmacKey(provider);

            if (hmacKey is null)
                return true;

            KeyValuePair<string, string> hmacKeyValuePair = queryStrings.Get(hmacKey);
            string hmacValue = hmacKeyValuePair.Value;
            queryStrings.Remove(hmacKeyValuePair);
            ProviderSetting providerSetting = GetProviderSettings(provider);

            return HmacCipher.Validate(queryStrings, hmacValue, providerSetting.AppSecret);
        }

        public PermanentAccessBase GetPermanentAccessBody(
            PosProviders provider,
            ProviderSetting providerSetting,
            string shortLivedCode)
        {
            return provider switch
            {
                PosProviders.Shopify => new ShopifyPermanentAccess(
                    providerSetting.AppId,
                    providerSetting.AppSecret,
                    shortLivedCode),
                _ => new PermanentAccessBase()
            };
        }

        public MiddlewareAuthorisationRequest GetMiddlewareAuthorisation(
            PosProviders provider,
            BasicAuthorisationDetail basicAuthorisationDetail,
            string? storeUrl = null)
        {
            SensitiveCallbackInfo sensitiveCallbackInfo = DecryptCallbackInfo(basicAuthorisationDetail.EncryptedUserInfo);

            return new MiddlewareAuthorisationRequest
            {
                Provider = provider.ToString(),
                StoreName = sensitiveCallbackInfo.Shop, // May need to consolidate store name and store url
                StoreUrl = storeUrl ?? sensitiveCallbackInfo.Shop, // Need to change to StoreUrl
                Login = basicAuthorisationDetail.Login,
                Password = basicAuthorisationDetail.Password,
                EntityId = sensitiveCallbackInfo.EntityId,
                UserId = sensitiveCallbackInfo.UserId,
                AirslipUserType = sensitiveCallbackInfo.AirslipUserType
            };
        }

        public async Task<MiddlewareAuthorisationRequest> QueryPermanentAccessToken(
            ProviderDetails providerDetails,
            ShortLivedAuthorisationDetail shortLivedAuthorisationDetail)
        {
            HttpClient httpClient =
                _httpClientFactory.CreateClient(providerDetails.Provider.ToString());

            PermanentAccessBase accessBody = GetPermanentAccessBody(
                providerDetails.Provider,
                providerDetails.ProviderSetting,
                shortLivedAuthorisationDetail.ShortLivedCode);

            HttpRequestMessage httpRequestMessage = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(shortLivedAuthorisationDetail.PermanentAccessUrl),
                Content = new StringContent(
                    Json.Serialize(accessBody),
                    Encoding.UTF8,
                    Json.MediaType)
            };

            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error(
                    "Error posting request to provider for Url {PostUrl}, response code: {StatusCode}",
                    shortLivedAuthorisationDetail.PermanentAccessUrl,
                    response.StatusCode);
                throw new Exception("Error getting permanent access token");
            }

            string content = await response.Content.ReadAsStringAsync();
            BasicAuthorisationDetail basicAuth = new();
            
            switch (providerDetails.Provider)
            {
                case PosProviders.Shopify:
                    basicAuth = Json.Deserialize<ShopifyBasicAuthorisationDetail>(content);
                    basicAuth.Login = providerDetails.ProviderSetting.AppSecret;
                    break;
            }
            
            basicAuth.Shop = shortLivedAuthorisationDetail.StoreName;
            basicAuth.EncryptedUserInfo = shortLivedAuthorisationDetail.EncryptedUserInfo;
            
            return GetMiddlewareAuthorisation(
                providerDetails.Provider,
                basicAuth,
                shortLivedAuthorisationDetail.BaseUri);
        }

        private static string GetInternalMiddlewareName(PosProviders provider)
        {
            return provider switch
            {
                PosProviders.Shopify => PosProviders.Api2Cart.ToString(),
                PosProviders.Squarespace => PosProviders.Api2Cart.ToString(),
                PosProviders.Volusion => PosProviders.Api2Cart.ToString(),
                PosProviders.WoocommerceApi => PosProviders.Api2Cart.ToString(),
                _ => provider.ToString()
            };
        }

        private static string? GetHmacKey(PosProviders provider)
        {
            return provider switch
            {
                PosProviders.WoocommerceApi => null,
                _ => "hmac"
            };
        }

        private (string cipherUrl, SensitiveCallbackInfo generateCallbackAuthRequest) GetEncryptedUserInformation(string queryString)
        {
            SensitiveCallbackInfo sensitiveCallbackAuthRequest = queryString.GetQueryParams<SensitiveCallbackInfo>();
          
            string serialisedUserInformation = Json.Serialize(sensitiveCallbackAuthRequest);

            string cipherUrl = StringCipher.EncryptForUrl(
                serialisedUserInformation,
                _encryptionOptions.Value.PassPhraseToken);
            
            return (cipherUrl, sensitiveCallbackAuthRequest);
        }

        public SensitiveCallbackInfo DecryptCallbackInfo(string cipherString)
        {
            string decryptedUserInfo = string.Empty;
            try
            {
                decryptedUserInfo = StringCipher.Decrypt(cipherString, _encryptionOptions.Value.PassPhraseToken);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                // WooCommerce replaces the Encoded values with spaces and removes the equals sign
                if (cipherString.Contains(' '))
                    cipherString = cipherString.Replace(" ", "+");
                
                decryptedUserInfo = StringCipher.Decrypt(cipherString, _encryptionOptions.Value.PassPhraseToken);
                return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if(cipherString.Last().ToString() != "=")
                    cipherString += "=";
                decryptedUserInfo = StringCipher.Decrypt(cipherString, _encryptionOptions.Value.PassPhraseToken);
            }
            catch (Exception)
            {
                // ignored
            }

            return Json.Deserialize<SensitiveCallbackInfo>(decryptedUserInfo);
        }
    }

    public class ShopifyProvider : IProvider
    {
        public bool IsOnline { get; set; }
        public string Shop { get; set; } = string.Empty;
    }

    public interface IProvider
    {
    }

    public class SquarespaceProvider : IProvider
    {
        public string Shop { get; set; } = string.Empty;
    }
    public class WooCommerceProvider : IProvider
    {
        public string Shop { get; set; } = string.Empty;
    }

    public class PermanentAccessBase
    {
        public virtual string AppId { get; set; } = string.Empty;
        public virtual string AppSecret { get; set; } = string.Empty;
        public virtual string ShortLivedCode { get; set; } = string.Empty;
    }

    public class ShopifyPermanentAccess : PermanentAccessBase
    {
        [JsonProperty(PropertyName = "client_id")]
        public sealed override string AppId { get; set; }

        [JsonProperty(PropertyName = "client_secret")]
        public sealed override string AppSecret { get; set; }

        [JsonProperty(PropertyName = "code")] public sealed override string ShortLivedCode { get; set; }

        public ShopifyPermanentAccess(
            string appId,
            string appSecret,
            string shortLivedCode)
        {
            AppId = appId;
            AppSecret = appSecret;
            ShortLivedCode = shortLivedCode;
        }
    }
}