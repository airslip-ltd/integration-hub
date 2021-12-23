using Airslip.Common.Auth.Interfaces;
using Airslip.Common.Auth.Models;
using Airslip.Common.Repository.Interfaces;
using Airslip.Common.Security.Configuration;
using Airslip.Common.Security.Implementations;
using Airslip.Common.Testing;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Types.Interfaces;
using Airslip.Common.Utilities;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using Xunit;

namespace Airslip.IntegrationHub.Services.UnitTests
{
    public class ProviderDiscoveryServiceTests
    {
        private readonly ProviderDiscoveryService _sut;
        private readonly Mock<IOptions<EncryptionSettings>> _encryptionSettings;

        public ProviderDiscoveryServiceTests()
        {
            string projectName = "Airslip.IntegrationHub";
            Mock<IOptions<SettingCollection<ProviderSetting>>> providerSettingsMock = new();
            Mock<IOptions<PublicApiSettings>> publicApiSettingsMock = OptionsMock.SetUpOptionSettings<PublicApiSettings>(projectName)!;
            Mock<ITokenDecodeService<ApiKeyToken>> tokenDecodeServiceMock = new();
            Mock<ILogger> loggerMock = new();
            _encryptionSettings = OptionsMock.SetUpOptionSettings<EncryptionSettings>(projectName)!;
            SettingCollection<ProviderSetting> settings = Factory.ProviderConfiguration;
            Mock<IHttpClientFactory> providerHttpClient = new();
            Mock<IMapper> providerAuthorisingDetailMock = new();
          
            providerSettingsMock
                .Setup(s => s.Value)
                .Returns(settings);

            tokenDecodeServiceMock
                .Setup(s => s.GetCurrentToken())
                .Returns(new ApiKeyToken {AirslipUserType = AirslipUserType.Merchant, EntityId = "entity-id"});

            providerAuthorisingDetailMock
                .Setup(s => s.Map<ProviderAuthorisingDetail>(It.IsAny<object>()))
                .Returns(Factory.ProviderAuthorisingDetail);

            _sut = new ProviderDiscoveryService(
                providerSettingsMock.Object,
                publicApiSettingsMock.Object,
                tokenDecodeServiceMock.Object,
                _encryptionSettings.Object,
                providerHttpClient.Object,
                providerAuthorisingDetailMock.Object,
                loggerMock.Object);
        }

        [Theory]
        [InlineData(PosProviders.Shopify,
            "?shop=airslip-development.myshopify.com&isOnline=true",
            "state",
            "https://airslip-development.myshopify.com/admin/oauth/authorize?client_id=client-id&scope=read_orders,read_products,read_inventory&redirect_uri=http://localhost:31201/v1/auth/callback&grant_options[]=per-user")]
        [InlineData(PosProviders.Vend,
            "",
            "state",
            "https://secure.vendhq.com/connect?response_type=code&client_id=SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL&redirect_uri=http://localhost:38101/v1/auth")]
        public void Can_generate_callback_url(PosProviders provider, string queryString, string relayQueryString,
            string expectedResult)
        {
            string callBackUrl = _sut.GenerateCallbackUrl(provider, queryString);

            string urlDecodedCallbackUrl = HttpUtility.UrlDecode(callBackUrl);

            List<KeyValuePair<string, string>> queryParams = urlDecodedCallbackUrl.GetQueryParams(true).ToList();

            KeyValuePair<string, string> keyValuePair = queryParams.Get(relayQueryString);
            queryParams.Remove(keyValuePair);

            urlDecodedCallbackUrl.Should().NotBeEmpty();

            int i = expectedResult.IndexOf("?", StringComparison.Ordinal);
            string queryWithoutQueryString = expectedResult.Substring(0, i);

            string queryStringWithoutState = queryParams.ToQueryStringUrl(queryWithoutQueryString);
            queryStringWithoutState.Should().Be(expectedResult);

            string serialisedEncryption =
                StringCipher.Decrypt(keyValuePair.Value, _encryptionSettings.Object.Value.PassPhraseToken);
            UserInformation userInformation = Json.Deserialize<UserInformation>(serialisedEncryption);
            userInformation.UserType.Should().Be(AirslipUserType.Merchant);
            userInformation.EntityId.Should().Be("entity-id");
        }

        [Fact]
        public void Can_override_redirect_uri_for_callback_url_generator()
        {
            string callBackUrl = _sut.GenerateCallbackUrl(PosProviders.Vend, "", redirectUri: "override-url");
            string urlDecodedCallbackUrl = HttpUtility.UrlDecode(callBackUrl);

            List<KeyValuePair<string, string>> queryParams = urlDecodedCallbackUrl.GetQueryParams(true).ToList();

            string overrideRedirectUri = queryParams.GetValue("redirect_uri");
            urlDecodedCallbackUrl.Should().NotBeEmpty();
            overrideRedirectUri.Should().Be("override-url");
        }

        [Theory]
        [InlineData("Shopify")]
        // [InlineData("Cart3D")]
        // [InlineData("Cart3DApi")]
        // [InlineData("AmazonSP")]
        // [InlineData("Amazon")]
        // [InlineData("Demandware")]
        // [InlineData("EBay")]
        // [InlineData("Etsy")]
        // [InlineData("EtsyAPIv3")]
        // [InlineData("Magento")]
        // [InlineData("Hybris")]
        public void Can_get_all_oauth_provider_details(string provider)
        {
            string queryString =
                "?code=9eb34b1a83917cee25bb0199c8711bab&hmac=24bd4063d10d51f5f117f8f9e936412cbc71a049400d3dd58b0407c8737b1bf3&host=YWlyc2xpcC1kZXZlbG9wbWVudC5teXNob3BpZnkuY29tL2FkbWlu&shop=airslip-development.myshopify.com&state=b951eMbRF6NelKyGXt8cRaj%2Fflv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV%2F5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1&timestamp=1639833474";
            ProviderDetails providerDetails = _sut.GetProviderDetails(provider, queryString);

            providerDetails.Provider.Should().Be(Enum.Parse<PosProviders>(provider));
            providerDetails.DestinationBaseUri.Should().Be("https://dev-integrations.airslip.com/api2cart/v1");
            providerDetails.AuthorisingDetail.PermanentAccessUrl.Should().Be("permanent-access-url");
            providerDetails.AuthorisingDetail.BaseUri.Should().Be("base-uri");
            providerDetails.AuthorisingDetail.AirslipUserInfo.Should().Be("airslip-user-info");
            providerDetails.AuthorisingDetail.StoreName.Should().Be("store-name");
            providerDetails.AuthorisingDetail.ShortLivedCode.Should().Be("short-lived-code");
        }
        
        [Theory]
        [InlineData("Volusion")]
        // [InlineData("AspDotNetStorefront")]
        // [InlineData("CommerceHQ")]
        // [InlineData("Ecwid")]
        // [InlineData("Neto")]
        // [InlineData("LightSpeed")]
        // [InlineData("Prestashop")]
        // [InlineData("Squarespace")]
        // [InlineData("Shopware")]
        // [InlineData("ShopwareApi")]
        // [InlineData("Walmart")]
        // [InlineData("Woocommerce")]
        public void Can_get_all_non_oauth_provider_details(string provider)
        {
            string queryString =
                "?code=9eb34b1a83917cee25bb0199c8711bab&hmac=24bd4063d10d51f5f117f8f9e936412cbc71a049400d3dd58b0407c8737b1bf3&host=YWlyc2xpcC1kZXZlbG9wbWVudC5teXNob3BpZnkuY29tL2FkbWlu&shop=airslip-development.myshopify.com&state=b951eMbRF6NelKyGXt8cRaj%2Fflv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV%2F5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1&timestamp=1639833474";
            ProviderDetails providerDetails = _sut.GetProviderDetails(provider, queryString);

            providerDetails.Provider.Should().Be(Enum.Parse<PosProviders>(provider));
            providerDetails.DestinationBaseUri.Should().Be("https://dev-integrations.airslip.com/api2cart/v1");
            providerDetails.AuthorisingDetail.PermanentAccessUrl.Should().BeEmpty();
            providerDetails.AuthorisingDetail.BaseUri.Should().BeNull();
            providerDetails.AuthorisingDetail.AirslipUserInfo.Should().BeEmpty();
            providerDetails.AuthorisingDetail.StoreName.Should().BeEmpty();
            providerDetails.AuthorisingDetail.ShortLivedCode.Should().BeEmpty();
        }
    }
    
    public static class Extensions
    {
        public static string ToQueryStringUrl(this IEnumerable<KeyValuePair<string, string>> source, string baseUrl)
        {
            string result = source.Aggregate(string.Empty,
                (current, keyValuePair) => current + "&" + keyValuePair.Key + "=" + keyValuePair.Value);

            return baseUrl + result.ReplaceFirst("&", "?");
        }
    }
}