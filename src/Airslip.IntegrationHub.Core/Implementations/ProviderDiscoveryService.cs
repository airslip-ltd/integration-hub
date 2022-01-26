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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
            (string cipheredSensitiveInfo, SensitiveCallbackInfo generateCallbackAuthRequest) = GetEncryptedUserInformation(queryString);
            
            string destinationBaseUri = _publicApiSettings.GetSettingByName("Base").ToBaseUri();
            string redirectUri = generateCallbackAuthRequest.CallbackUrl ?? 
                                 $"{destinationBaseUri}/auth/callback/{provider}";
            
            ProviderSetting providerSetting = GetProviderSettings(provider);

            string encodedScope = HttpUtility.UrlEncode(providerSetting.Scope);

            switch (provider)
            {
                case PosProviders.EBay:
                    return //   Potentialy encode scope
                        $"{providerSetting.BaseUri}?client_id={providerSetting.AppId}&response_type=code&redirect_uri={redirectUri}&scope={providerSetting.Scope}&state={cipheredSensitiveInfo}";
                case PosProviders.Vend:
                    return
                        $"{providerSetting.BaseUri}?response_type=code&client_id={providerSetting.AppId}&redirect_uri={redirectUri}&state={cipheredSensitiveInfo}";
                case PosProviders.SwanRetailMidas:
                    return string.Empty;
                case PosProviders.Volusion:
                    return string.Empty;
                case PosProviders.Shopify:
                    ShopifyProvider auth = queryString.GetQueryParams<ShopifyProvider>();
                    string grantOptions = auth.IsOnline ? "per-user" : "value";
                    return
                        $"{string.Format(providerSetting.FormatBaseUri(auth.Shop))}/admin/oauth/authorize?client_id={providerSetting.AppId}&scope={providerSetting.Scope}&redirect_uri={redirectUri}&state={cipheredSensitiveInfo}&grant_options[]={grantOptions}";
                case PosProviders.Stripe:
                case PosProviders.SumUp:
                case PosProviders.IZettle:
                case PosProviders.EposNow:
                case PosProviders.WoocommerceApi:
                    WooCommerceProvider wooCommerce = queryString.GetQueryParams<WooCommerceProvider>();
                    return
                        $"{string.Format(providerSetting.FormatBaseUri(wooCommerce.Shop))}/wc-auth/v1/authorize?app_name=Airslip&scope={providerSetting.Scope}&user_id={cipheredSensitiveInfo}&return_url=https://google.com&callback_url={redirectUri}";
                case PosProviders.Squarespace:
                    SquarespaceProvider squarespaceAuth = queryString.GetQueryParams<SquarespaceProvider>();
                    return
                        $"{string.Format(providerSetting.FormatBaseUri(squarespaceAuth.Shop))}/api/1/login/oauth/provider/authorize?client_id={providerSetting.AppId}&scope={providerSetting.Scope}&redirect_uri={redirectUri}&state={cipheredSensitiveInfo}&access_type=offline";
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, "Not yet supported");
            }
        }
        
        public bool ValidateHmac(
            PosProviders provider,
            List<KeyValuePair<string, string>> queryStrings)
        {
            // Need to add for WooCommerce
            string? hmacKey = GetHmacKey(provider);

            if (hmacKey is null)
                return true;

            KeyValuePair<string, string> hmacKeyValuePair = queryStrings.Get(hmacKey);
            string hmacValue = hmacKeyValuePair.Value;
            queryStrings.Remove(hmacKeyValuePair);
            ProviderSetting providerSetting = GetProviderSettings(provider);

            return HmacCipher.Validate(queryStrings, hmacValue, providerSetting.AppSecret);
        }

        private static PermanentAccessBase GetPermanentAccessBody(
            ProviderDetails providerDetails,
            string shortLivedCode)
        {
            return providerDetails.Provider switch
            {
                PosProviders.Shopify => new ShopifyPermanentAccess(
                    providerDetails.ProviderSetting.AppId,
                    providerDetails.ProviderSetting.AppSecret,
                    shortLivedCode),
                PosProviders.EBay => new EbayPermanentAccess(shortLivedCode, providerDetails.RedirectUri),
                _ => new PermanentAccessBase()
            };
        }

        public MiddlewareAuthorisationRequest GetMiddlewareAuthorisation(
            ProviderDetails providerDetails,
            BasicAuthorisationDetail basicAuthorisationDetail,
            string? storeUrl = null)
        {
            SensitiveCallbackInfo sensitiveCallbackInfo = DecryptCallbackInfo(basicAuthorisationDetail.EncryptedUserInfo);

            return new MiddlewareAuthorisationRequest
            {
                Provider = providerDetails.Provider.ToString(),
                StoreName = sensitiveCallbackInfo.Shop, // May need to consolidate store name and store url
                StoreUrl = providerDetails.ProviderSetting.FormatBaseUri(sensitiveCallbackInfo.Shop), // Need to change to StoreUrl
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
            // Change create client name
            
            PermanentAccessBase accessBody = GetPermanentAccessBody(
                providerDetails,
                shortLivedAuthorisationDetail.ShortLivedCode);
            
            HttpRequestMessage httpRequestMessage = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(shortLivedAuthorisationDetail.PermanentAccessUrl),
                Content = new StringContent(
                    Json.Serialize(accessBody),
                    Encoding.UTF8,
                    Json.MediaType)
                // TODO: Use Postman to validate it works then create function to get content
            //     Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[] 
            //     {
            //     new("redirect_uri", providerDetails.RedirectUri),
            //     new("grant_type", accessBody.GrantType!),
            //     new("code", accessBody.ShortLivedCode),
            // }) //application/x-www-form-urlencoded
            };

            // Change to function
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes( $"{providerDetails.ProviderSetting.AppId}:{providerDetails.ProviderSetting.AppSecret}")));
            
            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error(
                    "Error posting request to provider for Url {PostUrl}, response code: {StatusCode}, Error: {ErrorResponse}",
                    shortLivedAuthorisationDetail.PermanentAccessUrl,
                    response.StatusCode,
                    content);
                throw new Exception("Error getting permanent access token");
            }

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
                providerDetails,
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
                PosProviders.EBay => PosProviders.Api2Cart.ToString(),
                _ => provider.ToString()
            };
        }

        private static string? GetHmacKey(PosProviders provider)
        {
            return provider switch
            {
                PosProviders.Shopify => "hmac",
                _ => null
            };
        }

        private (string cipheredSensitiveInfo, SensitiveCallbackInfo generateCallbackAuthRequest) GetEncryptedUserInformation(string queryString)
        {
            SensitiveCallbackInfo sensitiveCallbackAuthRequest = queryString.GetQueryParams<SensitiveCallbackInfo>();
          
            string serialisedUserInformation = Json.Serialize(sensitiveCallbackAuthRequest);

            string cipheredSensitiveInfo = StringCipher.EncryptForUrl(
                serialisedUserInformation,
                _encryptionOptions.Value.PassPhraseToken);
            
            return (cipheredSensitiveInfo, sensitiveCallbackAuthRequest);
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
        public virtual string? AppId { get; set; } 
        public virtual string? AppSecret { get; set; }
        public virtual string ShortLivedCode { get; set; } = string.Empty;
        public virtual string? GrantType { get; set; }
    }

    public class ShopifyPermanentAccess : PermanentAccessBase
    {
        [JsonProperty(PropertyName = "client_id")]
        public sealed override string? AppId { get; set; }

        [JsonProperty(PropertyName = "client_secret")]
        public sealed override string? AppSecret { get; set; }

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
    
    public class EbayPermanentAccess : PermanentAccessBase
    {
        [JsonProperty(PropertyName = "grant_type")]
        public sealed override string? GrantType { get; set; } = "authorization_code";

        [JsonProperty(PropertyName = "code")] 
        public sealed override string ShortLivedCode { get; set; }
        
        [JsonProperty(PropertyName = "redirect_uri")] 
        public string RedirectUrl { get; set; }

        public EbayPermanentAccess(
            string shortLivedCode,
            string redirectUrl)
        {
            ShortLivedCode = shortLivedCode;
            RedirectUrl = redirectUrl;
        }
    }
}