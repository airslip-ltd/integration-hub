using Airslip.Common.Security.Configuration;
using Airslip.Common.Security.Implementations;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using Airslip.IntegrationHub.Core.Requests;
using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
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

        public ProviderDetails GetProviderDetails(string provider, string queryString)
        {
            ProviderSetting providerSetting = GetProviderSettings(provider);
            PosProviders posProvider = Enum.Parse<PosProviders>(provider);
            
            string internalMiddlewareName = GetInternalProviderName(posProvider);
            
            PublicApiSetting publicApiSetting = _publicApiSettings.GetSettingByName(internalMiddlewareName);
            string destinationBaseUri = publicApiSetting.ToBaseUri();
            string redirectUri = $"{destinationBaseUri}/auth/callback/{provider}";
            
            ProviderAuthorisingDetail authorisingDetail = new();
            switch (posProvider)
            {
                case PosProviders.Shopify:
                    ShopifyProviderAuthorisingDetail shopifyParams = queryString.GetQueryParams<ShopifyProviderAuthorisingDetail>();
                    authorisingDetail = shopifyParams;
                    authorisingDetail.BaseUri = string.Format(providerSetting.BaseUri, shopifyParams.Shop);
                    authorisingDetail.PermanentAccessUrl = $"https://{shopifyParams.Shop}/admin/oauth/access_token";
                    authorisingDetail.StoreName = shopifyParams.Shop.Replace(".myshopify.com", "");
                    break;
                case PosProviders.Squarespace:
                    authorisingDetail = queryString.GetQueryParams<SquarespaceProviderAuthorisingDetail>();
                    authorisingDetail.PermanentAccessUrl = "https://login.squarespace.com/api/1/login/oauth/provider/tokens";
                    break;
            }

            return new ProviderDetails(
                posProvider, 
                destinationBaseUri,
                redirectUri,
                publicApiSetting, 
                providerSetting,
                authorisingDetail);
        }

        public ProviderSetting GetProviderSettings(string provider)
        {
            return _providerSettings.GetSettingByName(provider);
        }

        public string GenerateCallbackUrl(PosProviders provider, string queryString)
        {
            Tuple<string, UserInformation> encryptedUserInformation = GetEncryptedUserInformation(queryString);
            
            string destinationBaseUri = _publicApiSettings.GetSettingByName("Base").ToBaseUri();
            string redirectUri = encryptedUserInformation.Item2.CallbackUrl ?? 
                                 $"{destinationBaseUri}/auth/callback/{provider}";
            
            ProviderSetting providerSetting = GetProviderSettings(provider.ToString());

            switch (provider)
            {
                case PosProviders.Vend:
                    return
                        $"{providerSetting.BaseUri}?response_type=code&client_id={providerSetting.AppId}&redirect_uri={redirectUri}&state={encryptedUserInformation.Item1}";
                case PosProviders.SwanRetailMidas:
                    return string.Empty;
                case PosProviders.Volusion:
                    return string.Empty;
                case PosProviders.Shopify:
                    ShopifyProvider auth = queryString.GetQueryParams<ShopifyProvider>();
                    string grantOptions = auth.IsOnline ? "per-user" : "value";
                    return
                        $"{string.Format(providerSetting.BaseUri, auth.Shop)}/admin/oauth/authorize?client_id={providerSetting.AppId}&scope={providerSetting.Scope}&redirect_uri={redirectUri}&state={encryptedUserInformation.Item1}&grant_options[]={grantOptions}";
                case PosProviders.Stripe:
                case PosProviders.SumUp:
                case PosProviders.IZettle:
                case PosProviders.EposNow:
                case PosProviders.Square:
                case PosProviders.Squarespace:
                    SquarespaceProvider squarespaceAuth = queryString.GetQueryParams<SquarespaceProvider>();
                    return
                        $"{string.Format(providerSetting.BaseUri, squarespaceAuth.Shop)}/api/1/login/oauth/provider/authorize?client_id={providerSetting.AppId}&scope={providerSetting.Scope}&redirect_uri={redirectUri}&state={encryptedUserInformation.Item1}&access_type=offline";
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, "Not yet supported");
            }
        }

        public bool Validate(PosProviders provider, List<KeyValuePair<string, string>> queryStrings)
        {
            string hmacKey = GetHmacKey(provider);

            KeyValuePair<string, string> hmacKeyValuePair = queryStrings.Get(hmacKey);
            string hmacValue = hmacKeyValuePair.Value;
            queryStrings.Remove(hmacKeyValuePair);
            ProviderSetting providerSetting = GetProviderSettings(provider.ToString());

            return HmacCipher.Validate(queryStrings, hmacValue, providerSetting.AppSecret);
        }

        public Task<MiddlewareAuthorisationRequest> GetBody(string provider)
        {
            return Task.Run(() => new MiddlewareAuthorisationRequest());
        }

        public PermanentAccessBase GetPermanentAccessBody(PosProviders provider, ProviderSetting providerSetting, string shortLivedCode)
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

        public async Task<MiddlewareAuthorisationRequest> QueryPermanentAccessToken(string provider, ProviderDetails providerDetails)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(provider);
            
            PermanentAccessBase accessBody = GetPermanentAccessBody(
                providerDetails.Provider,  
                providerDetails.ProviderSetting,
                providerDetails.AuthorisingDetail.ShortLivedCode);

            HttpRequestMessage httpRequestMessage = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(providerDetails.AuthorisingDetail.PermanentAccessUrl),
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
                    providerDetails.AuthorisingDetail.PermanentAccessUrl, 
                    response.StatusCode);
                throw new Exception("Error getting permanent access token");
            }
            
            string content = await response.Content.ReadAsStringAsync();
            ProviderAuthorisation providerAuthResponse = GetProviderAuthResponse(providerDetails.Provider, 
                providerDetails.ProviderSetting, content);

            string decryptedUserInfo = StringCipher.Decrypt(providerDetails.AuthorisingDetail.EncryptedUserInfo, 
                _encryptionOptions.Value.PassPhraseToken);
            UserInformation userInformation = Json.Deserialize<UserInformation>(decryptedUserInfo);
            
            return new MiddlewareAuthorisationRequest
            {
                Provider = provider,
                StoreName =providerDetails.AuthorisingDetail.StoreName,
                StoreUrl =  providerDetails.AuthorisingDetail.BaseUri ?? providerDetails.ProviderSetting.BaseUri,
                Login = providerAuthResponse.Login,
                Password = providerAuthResponse.Password,
                EntityId = userInformation.EntityId,
                UserId = userInformation.UserId,
                AirslipUserType = userInformation.AirslipUserType
            };
        }

        private static ProviderAuthorisation GetProviderAuthResponse(PosProviders posProvider, ProviderSetting providerSettings, string content)
        {
            switch (posProvider)
            {
                case PosProviders.Shopify:
                    ShopifyProviderAuthorisation providerAuth = Json.Deserialize<ShopifyProviderAuthorisation>(content);
                    providerAuth.Login = providerSettings.AppSecret;
                    return providerAuth;
                default:
                    return Json.Deserialize<ProviderAuthorisation>(content);
            }
        }

        private static string GetInternalProviderName(PosProviders provider)
        {
            return provider switch
            {
                PosProviders.Shopify => PosProviders.Api2Cart.ToString(),
                PosProviders.Squarespace => PosProviders.Api2Cart.ToString(),
                PosProviders.Volusion => PosProviders.Api2Cart.ToString(),
                _ => provider.ToString()
            };
        }

        private static string GetHmacKey(PosProviders provider)
        {
            return provider switch
            {
                _ => "hmac"
            };
        }

        private Tuple<string, UserInformation> GetEncryptedUserInformation(string queryString)
        {
            UserInformation userInformation = queryString.GetQueryParams<UserInformation>();

            string serialisedUserInformation = Json.Serialize(userInformation);

            return new Tuple<string, UserInformation>(StringCipher.EncryptForUrl(serialisedUserInformation, _encryptionOptions.Value.PassPhraseToken),
                userInformation);
        }
    }

    public class ShopifyProvider : IProvider
    {
        public bool IsOnline { get; set; }
        public string Shop { get; set; } = string.Empty;
    }
    
    public class UserAuthRequest
    {
        public AirslipUserType AirslipUserType { get; set; } = AirslipUserType.Standard;
        public string EntityId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public interface IProvider
    {
        
    }
    
    public class SquarespaceProvider : IProvider
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
        [JsonProperty(PropertyName = "code")]
        public sealed override string ShortLivedCode { get; set; }

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