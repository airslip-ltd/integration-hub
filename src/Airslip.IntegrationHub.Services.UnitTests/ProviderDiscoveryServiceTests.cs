using Airslip.Common.Security.Configuration;
using Airslip.Common.Testing;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
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

        public ProviderDiscoveryServiceTests()
        {
            string projectName = "Airslip.IntegrationHub";
            Mock<IOptions<SettingCollection<ProviderSetting>>> providerSettingsMock = new();
            Mock<IOptions<PublicApiSettings>> publicApiSettingsMock = OptionsMock.SetUpOptionSettings<PublicApiSettings>(projectName)!;
            Mock<ILogger> loggerMock = new();
            Mock<IOptions<EncryptionSettings>> encryptionSettings = OptionsMock.SetUpOptionSettings<EncryptionSettings>(projectName)!;
            SettingCollection<ProviderSetting> settings = Factory.ProviderConfiguration;
            Mock<IHttpClientFactory> providerHttpClient = new();

            providerSettingsMock
                .Setup(s => s.Value)
                .Returns(settings);

            _sut = new ProviderDiscoveryService(
                providerSettingsMock.Object,
                publicApiSettingsMock.Object,
                encryptionSettings.Object,
                providerHttpClient.Object,
                loggerMock.Object);
        }

        [Theory]
        [InlineData(PosProviders.Shopify,
            "?shop=airslip-development.myshopify.com&isOnline=true",
            "state",
            "https://airslip-development.myshopify.com/admin/oauth/authorize?client_id=client-id&scope=read_orders,read_products,read_inventory&redirect_uri=https://dev-integrations.airslip.com/oauth/v1/auth/callback/Shopify&grant_options[]=per-user")]
        [InlineData(PosProviders.Vend,
            "",
            "state",
            "https://secure.vendhq.com/connect?response_type=code&client_id=SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL&redirect_uri=https://dev-integrations.airslip.com/oauth/v1/auth/callback/Vend")]
        [InlineData(PosProviders.Squarespace,
            "?shop=airslip-development",
            "state",
            "https://airslip-development.squarespace.com/api/1/login/oauth/provider/authorize?client_id=client-id&scope=website.orders.read,website.transactions.read,website.inventory.read,website.products.read&redirect_uri=https://dev-integrations.airslip.com/oauth/v1/auth/callback/Squarespace&access_type=offline")]
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
        [InlineData("Shopify",
            "?code=9eb34b1a83917cee25bb0199c8711bab&hmac=24bd4063d10d51f5f117f8f9e936412cbc71a049400d3dd58b0407c8737b1bf3&host=YWlyc2xpcC1kZXZlbG9wbWVudC5teXNob3BpZnkuY29tL2FkbWlu&shop=airslip-development.myshopify.com&state=b951eMbRF6NelKyGXt8cRaj%2Fflv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV%2F5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1&timestamp=1639833474",
            "https://dev-integrations.airslip.com/api2cart/v1",
            "https://airslip-development.myshopify.com/admin/oauth/access_token",
            "https://airslip-development.myshopify.com")]
        // [InlineData("Squarespace",
        //     "?code=9eb34b1a83917cee25bb0199c8711bab&shop=airslip-development.squarespace.com&state=b951eMbRF6NelKyGXt8cRaj%2Fflv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV%2F5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1",
        //     "https://dev-integrations.airslip.com/api2cart/v1",
        //     "https://login.squarespace.com/api/1/login/oauth/provider/tokens",
        //     "")]
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
        public void Can_get_all_oauth_provider_details(string provider, string queryString, string destinationUrl, string permanentAccessUrl, string baseUri)
        {
            ProviderDetails providerDetails = _sut.GetProviderDetails(provider, queryString);
            providerDetails.Provider.Should().Be(Enum.Parse<PosProviders>(provider));
            providerDetails.DestinationBaseUri.Should().Be(destinationUrl);
            providerDetails.AuthorisingDetail.PermanentAccessUrl.Should().Be(permanentAccessUrl);    
            providerDetails.AuthorisingDetail.EncryptedUserInfo.Should().Be("b951eMbRF6NelKyGXt8cRaj/flv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV/5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1");
            providerDetails.AuthorisingDetail.ShortLivedCode.Should().Be("9eb34b1a83917cee25bb0199c8711bab");
            //TODO: Need to run full squarespace flow to understand where store name will come from, may have to pass through the state
            providerDetails.AuthorisingDetail.StoreName.Should().Be("airslip-development");
            providerDetails.AuthorisingDetail.BaseUri.Should().Be(baseUri); //    null
        }

        [Theory]
        [InlineData("Volusion")]
        // [InlineData("AspDotNetStorefront")]
        // [InlineData("CommerceHQ")]
        // [InlineData("Ecwid")]
        // [InlineData("Neto")]
        // [InlineData("LightSpeed")]
        // [InlineData("Prestashop")]
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
            providerDetails.AuthorisingDetail.EncryptedUserInfo.Should().BeEmpty();
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