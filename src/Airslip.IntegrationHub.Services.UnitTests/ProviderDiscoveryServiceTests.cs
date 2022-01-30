using Airslip.Common.Security.Configuration;
using Airslip.Common.Testing;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.CompilerServices;
using Moq;
using System.Collections.Generic;
using System.Linq;
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

            IConfiguration appSettingsConfig = OptionsMock.InitialiseConfiguration(projectName)!;
            
            SettingCollection<ProviderSetting> providerSetting = new();
            appSettingsConfig.GetSection("ProviderSettings").Bind(providerSetting);
            
        //   SettingCollection<ProviderSetting> providerSettings = new();

        string json = "a:1:{s:8:\"Settings";a:15:{s:6:"Cart3D";a:4:{s:7:"BaseUri";s:0:"";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:12:"AuthStrategy";s:5:"Basic";}s:9:"Cart3DApi";a:4:{s:7:"BaseUri";s:0:"";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:12:"AuthStrategy";s:5:"Basic";}s:8:"AmazonSP";a:4:{s:7:"BaseUri";s:0:"";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:12:"AuthStrategy";s:10:"ShortLived";}s:6:"Amazon";a:4:{s:7:"BaseUri";s:0:"";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:12:"AuthStrategy";s:10:"ShortLived";}s:14:"BigcommerceApi";a:5:{s:7:"BaseUri";s:27:"https://api.bigcommerce.com";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:5:"Scope";s:85:"store_v2_orders_read_only,store_v2_transactions_read_only,store_v2_products_read_only";s:12:"AuthStrategy";s:10:"ShortLived";}s:10:"Demandware";a:4:{s:7:"BaseUri";s:0:"";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:12:"AuthStrategy";s:10:"ShortLived";}s:4:"EBay";a:8:{s:7:"BaseUri";s:28:"https://api.sandbox.ebay.com";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:5:"Scope";s:1796:"https://api.ebay.com/oauth/api_scope https://api.ebay.com/oauth/api_scope/buy.order.readonly https://api.ebay.com/oauth/api_scope/buy.guest.order https://api.ebay.com/oauth/api_scope/sell.marketing.readonly https://api.ebay.com/oauth/api_scope/sell.marketing https://api.ebay.com/oauth/api_scope/sell.inventory.readonly https://api.ebay.com/oauth/api_scope/sell.inventory https://api.ebay.com/oauth/api_scope/sell.account.readonly https://api.ebay.com/oauth/api_scope/sell.account https://api.ebay.com/oauth/api_scope/sell.fulfillment.readonly https://api.ebay.com/oauth/api_scope/sell.fulfillment https://api.ebay.com/oauth/api_scope/sell.analytics.readonly https://api.ebay.com/oauth/api_scope/sell.marketplace.insights.readonly https://api.ebay.com/oauth/api_scope/commerce.catalog.readonly https://api.ebay.com/oauth/api_scope/buy.shopping.cart https://api.ebay.com/oauth/api_scope/buy.offer.auction https://api.ebay.com/oauth/api_scope/commerce.identity.readonly https://api.ebay.com/oauth/api_scope/commerce.identity.email.readonly https://api.ebay.com/oauth/api_scope/commerce.identity.phone.readonly https://api.ebay.com/oauth/api_scope/commerce.identity.address.readonly https://api.ebay.com/oauth/api_scope/commerce.identity.name.readonly https://api.ebay.com/oauth/api_scope/commerce.identity.status.readonly https://api.ebay.com/oauth/api_scope/sell.finances https://api.ebay.com/oauth/api_scope/sell.item.draft https://api.ebay.com/oauth/api_scope/sell.payment.dispute https://api.ebay.com/oauth/api_scope/sell.item https://api.ebay.com/oauth/api_scope/sell.reputation https://api.ebay.com/oauth/api_scope/sell.reputation.readonly https://api.ebay.com/oauth/api_scope/commerce.notification.subscription https://api.ebay.com/oauth/api_scope/commerce.notification.subscription.readonly";s:12:"AuthStrategy";s:10:"ShortLived";s:7:"AppName";s:37:"Airslip_Limited-AirslipL-airsli-biogv";s:11:"Environment";s:7:"sandbox";s:10:"LocationId";s:1:"3";}s:4:"Etsy";a:4:{s:7:"BaseUri";s:0:"";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:12:"AuthStrategy";s:10:"ShortLived";}s:9:"EtsyAPIv3";a:5:{s:7:"BaseUri";s:20:"https://www.etsy.com";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:5:"Scope";s:25:"transactions_r listings_r";s:12:"AuthStrategy";s:10:"ShortLived";}s:7:"Magento";a:4:{s:7:"BaseUri";s:0:"";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:12:"AuthStrategy";s:10:"ShortLived";}s:6:"Hybris";a:4:{s:7:"BaseUri";s:0:"";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:12:"AuthStrategy";s:5:"Basic";}s:7:"Shopify";a:5:{s:7:"BaseUri";s:11:"https://{0}";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:5:"Scope";s:40:"read_orders,read_products,read_inventory";s:12:"AuthStrategy";s:10:"ShortLived";}s:11:"Squarespace";a:5:{s:7:"BaseUri";s:27:"https://{0}.squarespace.com";s:5:"AppId";s:0:"";s:9:"AppSecret";s:0:"";s:5:"Scope";s:90:"website.orders.read,website.transactions.read,website.inventory.read,website.products.read";s:12:"AuthStrategy";s:10:"ShortLived";}s:4:"Vend";a:4:{s:7:"BaseUri";s:33:"https://secure.vendhq.com/connect";s:5:"AppId";s:32:"SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL";s:9:"AppSecret";s:32:"yujZrOdVKZbGXUvfYP6VjWYluZJ77ge4";s:12:"AuthStrategy";s:10:"ShortLived";}s:14:"WoocommerceApi";a:3:{s:7:"BaseUri";s:11:"https://{0}";s:5:"Scope";s:10:"read_write";s:12:"AuthStrategy";s:5:"Basic";}}}";
           
            // providerSettingsMock
            //     .Setup(s => s.Value)
            //     .Returns(settings);
        
            _sut = new ProviderDiscoveryService(
                providerSettingsMock.Object,
                publicApiSettingsMock.Object);
        }

        [Fact]
        public void Test()
        {
            
        }

        // [Theory]
        // [InlineData(PosProviders.Shopify,
        //     "?shop=airslip-development.myshopify.com&isOnline=true",
        //     "state",
        //     "https://airslip-development.myshopify.com/admin/oauth/authorize?client_id=client-id&scope=read_orders,read_products,read_inventory&redirect_uri=https://dev-integrations.airslip.com/oauth/v1/auth/callback/Shopify&grant_options[]=per-user")]
        // [InlineData(PosProviders.Vend,
        //     "",
        //     "state",
        //     "https://secure.vendhq.com/connect?response_type=code&client_id=SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL&redirect_uri=https://dev-integrations.airslip.com/oauth/v1/auth/callback/Vend")]
        // [InlineData(PosProviders.Squarespace,
        //     "?shop=airslip-development",
        //     "state",
        //     "https://airslip-development.squarespace.com/api/1/login/oauth/provider/authorize?client_id=client-id&scope=website.orders.read,website.transactions.read,website.inventory.read,website.products.read&redirect_uri=https://dev-integrations.airslip.com/oauth/v1/auth/callback/Squarespace&access_type=offline")]
        // public void Can_generate_callback_url(PosProviders provider, string queryString, string relayQueryString,
        //     string expectedResult)
        // {
        //     string callBackUrl = _sut.GenerateCallbackUrl(provider, queryString);
        //
        //     string urlDecodedCallbackUrl = HttpUtility.UrlDecode(callBackUrl);
        //
        //     List<KeyValuePair<string, string>> queryParams = urlDecodedCallbackUrl.GetQueryParams(true).ToList();
        //
        //     KeyValuePair<string, string> keyValuePair = queryParams.Get(relayQueryString);
        //     queryParams.Remove(keyValuePair);
        //
        //     urlDecodedCallbackUrl.Should().NotBeEmpty();
        //
        //     int i = expectedResult.IndexOf("?", StringComparison.Ordinal);
        //     string queryWithoutQueryString = expectedResult.Substring(0, i);
        //
        //     string queryStringWithoutState = queryParams.ToQueryStringUrl(queryWithoutQueryString);
        //     queryStringWithoutState.Should().Be(expectedResult);
        // }

        // [Fact]
        // public void Can_override_redirect_uri_for_callback_url_generator()
        // {
        //     string callBackUrl = _sut.GenerateCallbackUrl(PosProviders.Vend, "?callbackUrl=override-url");
        //     string urlDecodedCallbackUrl = HttpUtility.UrlDecode(callBackUrl);
        //
        //     List<KeyValuePair<string, string>> queryParams = urlDecodedCallbackUrl.GetQueryParams(true).ToList();
        //
        //     string overrideRedirectUri = queryParams.GetValue("redirect_uri");
        //     urlDecodedCallbackUrl.Should().NotBeEmpty();
        //     overrideRedirectUri.Should().Be("override-url");
        // }

        // [Theory]
        // [InlineData("Shopify",
        //     "?code=9eb34b1a83917cee25bb0199c8711bab&hmac=24bd4063d10d51f5f117f8f9e936412cbc71a049400d3dd58b0407c8737b1bf3&host=YWlyc2xpcC1kZXZlbG9wbWVudC5teXNob3BpZnkuY29tL2FkbWlu&shop=airslip-development.myshopify.com&state=b951eMbRF6NelKyGXt8cRaj%2Fflv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV%2F5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1&timestamp=1639833474",
        //     "https://dev-integrations.airslip.com/api2cart/v1",
        //     "https://airslip-development.myshopify.com/admin/oauth/access_token",
        //     "https://airslip-development.myshopify.com")]
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
        // public void Can_get_all_oauth_provider_details(string provider, string queryString, string destinationUrl, string permanentAccessUrl, string baseUri)
        // {
        //     ProviderDetails providerDetails = _sut.GetProviderDetails(provider, queryString);
        //     providerDetails.Provider.Should().Be(Enum.Parse<PosProviders>(provider));
        //     providerDetails.DestinationBaseUri.Should().Be(destinationUrl);
        //     providerDetails.AuthorisingDetail.PermanentAccessUrl.Should().Be(permanentAccessUrl);    
        //     providerDetails.AuthorisingDetail.EncryptedUserInfo.Should().Be("b951eMbRF6NelKyGXt8cRaj/flv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV/5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1");
        //     providerDetails.AuthorisingDetail.ShortLivedCode.Should().Be("9eb34b1a83917cee25bb0199c8711bab");
        //     //TODO: Need to run full squarespace flow to understand where store name will come from, may have to pass through the state
        //     providerDetails.AuthorisingDetail.StoreName.Should().Be("airslip-development");
        //     providerDetails.AuthorisingDetail.BaseUri.Should().Be(baseUri); //    null
        // }

        //[Theory]
        //[InlineData("Volusion")]
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
        // public void Can_get_all_non_oauth_provider_details(string provider)
        // {
        //     string queryString =
        //         "?code=9eb34b1a83917cee25bb0199c8711bab&hmac=24bd4063d10d51f5f117f8f9e936412cbc71a049400d3dd58b0407c8737b1bf3&host=YWlyc2xpcC1kZXZlbG9wbWVudC5teXNob3BpZnkuY29tL2FkbWlu&shop=airslip-development.myshopify.com&state=b951eMbRF6NelKyGXt8cRaj%2Fflv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV%2F5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1&timestamp=1639833474";
        //     ProviderDetails providerDetails = _sut.GetProviderDetails(provider, queryString);
        //
        //     providerDetails.Provider.Should().Be(Enum.Parse<PosProviders>(provider));
        //     providerDetails.DestinationBaseUri.Should().Be("https://dev-integrations.airslip.com/api2cart/v1");
        //     providerDetails.AuthorisingDetail.PermanentAccessUrl.Should().BeEmpty();
        //     providerDetails.AuthorisingDetail.BaseUri.Should().BeNull();
        //     providerDetails.AuthorisingDetail.EncryptedUserInfo.Should().BeEmpty();
        //     providerDetails.AuthorisingDetail.StoreName.Should().BeEmpty();
        //     providerDetails.AuthorisingDetail.ShortLivedCode.Should().BeEmpty();
        // }
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