using Airslip.Common.Auth.Interfaces;
using Airslip.Common.Auth.Models;
using Airslip.Common.Repository.Interfaces;
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
        private readonly ITokenDecodeService<ApiKeyToken> _tokenDecodeService;
        private readonly SettingCollection<ProviderSetting> _providerSettings;
        private readonly PublicApiSettings _publicApiSettings;
        private readonly IOptions<EncryptionSettings> _encryptionOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProviderDiscoveryService(
            IOptions<SettingCollection<ProviderSetting>> providerOptions,
            IOptions<PublicApiSettings> publicApiOptions,
            ITokenDecodeService<ApiKeyToken> tokenDecodeService,
            IOptions<EncryptionSettings> encryptionOptions,
            IHttpClientFactory httpClientFactory, 
            IMapper mapper,
            ILogger logger)
        {
            _tokenDecodeService = tokenDecodeService;
            _encryptionOptions = encryptionOptions;
            _providerSettings = providerOptions.Value;
            _publicApiSettings = publicApiOptions.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _mapper = mapper;
        }

        public ProviderDetails GetProviderDetails(string provider, string queryString)
        {
            ProviderSetting providerSetting = GetProviderSettings(provider);
            PosProviders posProvider = Enum.Parse<PosProviders>(provider);
            
            string internalMiddlewareName = GetInternalProviderName(posProvider);
            
            PublicApiSetting publicApiSetting = _publicApiSettings.GetSettingByName(internalMiddlewareName);
            string destinationBaseUri = publicApiSetting.ToBaseUri();

            ProviderAuthorisingDetail authorisingDetail = new();
            switch (posProvider)
            {
                case PosProviders.Shopify:
                    ShopifyProviderAuthorisingDetail parameters = queryString.GetQueryParams<ShopifyProviderAuthorisingDetail>();
                    authorisingDetail = _mapper.Map<ProviderAuthorisingDetail>(parameters);
                    break;
            }

            return new ProviderDetails(
                posProvider, 
                destinationBaseUri, 
                publicApiSetting, 
                providerSetting,
                authorisingDetail);
        }

        public ProviderSetting GetProviderSettings(string provider)
        {
            return _providerSettings.GetSettingByName(provider);
        }

        public string GenerateCallbackUrl(PosProviders provider, string queryString, string? redirectUri = null)
        {
            ProviderSetting providerSetting = GetProviderSettings(provider.ToString());
            redirectUri ??= providerSetting.RedirectUri;

            string encryptedUserInformation = GetEncryptedUserInformation();

            switch (provider)
            {
                case PosProviders.Vend:
                    return
                        $"{providerSetting.BaseUri}?response_type=code&client_id={providerSetting.AppId}&redirect_uri={redirectUri}&state={encryptedUserInformation}";
                case PosProviders.SwanRetailMidas:
                    return string.Empty;
                case PosProviders.Volusion:
                    return string.Empty;
                case PosProviders.Shopify:
                    ShopifyProvider auth = queryString.GetQueryParams<ShopifyProvider>();
                    string grantOptions = auth.IsOnline ? "per-user" : "value";
                    return
                        $"{string.Format(providerSetting.BaseUri, auth.Shop)}/admin/oauth/authorize?client_id={providerSetting.AppId}&scope=read_orders,read_products,read_inventory&redirect_uri={redirectUri}&state={encryptedUserInformation}&grant_options[]={grantOptions}";
                case PosProviders.Stripe:
                case PosProviders.SumUp:
                case PosProviders.IZettle:
                case PosProviders.EposNow:
                case PosProviders.Square:
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
            }
            
            string content = await response.Content.ReadAsStringAsync();
            ProviderAuthorisation providerAuthResponse = GetProviderAuthResponse(providerDetails.Provider, providerDetails.ProviderSetting, content);
            
            return new MiddlewareAuthorisationRequest
            {
                Provider = provider,
                StoreName =providerDetails.AuthorisingDetail.StoreName,
                StoreUrl =  providerDetails.AuthorisingDetail.BaseUri ?? providerDetails.ProviderSetting.BaseUri,
                Login = providerAuthResponse.Login,
                Password = providerAuthResponse.Password,
                Reference = providerDetails.AuthorisingDetail.AirslipUserInfo
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

        private string GetEncryptedUserInformation()
        {
            ApiKeyToken apiKeyToken = _tokenDecodeService.GetCurrentToken();

            UserInformation userInformation = new(apiKeyToken.AirslipUserType, apiKeyToken.EntityId);
            string serialisedUserInformation = Json.Serialize(userInformation);

            return StringCipher.EncryptForUrl(serialisedUserInformation, _encryptionOptions.Value.PassPhraseToken);
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