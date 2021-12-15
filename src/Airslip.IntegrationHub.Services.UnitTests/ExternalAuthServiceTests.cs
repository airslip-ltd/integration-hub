using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Airslip.IntegrationHub.Services.UnitTests
{
    public class ExternalAuthServiceTests
    {
        private readonly ProviderDiscoveryService _sut;

        public ExternalAuthServiceTests()
        {
            Mock<IOptions<SettingCollection<ProviderSetting>>> providerSettingsMock = new();
            Mock<IOptions<PublicApiSettings>> publicApiSettingsMock = new();
            SettingCollection<ProviderSetting> settings = Factory.ProviderConfiguration;

            providerSettingsMock
                .Setup(s => s.Value)
                .Returns(settings);

            _sut = new ProviderDiscoveryService(providerSettingsMock.Object, publicApiSettingsMock.Object);
        }

        [Theory]
        [InlineData(PosProviders.Shopify,
            "https://airslip-development.myshopify.com/admin/oauth/authorize?client_id=client-id&scope=read_all_orders,read_products,read_inventory&redirect_uri=http://localhost:7071/v1/auth&state=account-id&grant_options[]=per-user",
            "account-id", "airslip-development", true)]
        [InlineData(PosProviders.Vend,
            "https://secure.vendhq.com/connect?response_type=code&client_id=SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL&redirect_uri=http://localhost:38101/v1/auth&state=account-id",
            "account-id")]
        public void Can_generate_callback_url(PosProviders provider, string expectedResult, string accountId,
            string? shopName = null, bool? isOnline = null)
        {
            string callBackUrl = _sut.GenerateCallbackUrl(provider, accountId, shopName, isOnline);

            callBackUrl.Should().NotBeEmpty();
            callBackUrl.Should().Be(expectedResult);
        }

        [Fact]
        public void Can_override_redirect_uri_for_callback_url_generator()
        {
            string callBackUrl = _sut.GenerateCallbackUrl(PosProviders.Vend, "account-id", redirectUri: "override-url");
            string url =
                "https://secure.vendhq.com/connect?response_type=code&client_id=SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL&redirect_uri=override-url&state=account-id";

            callBackUrl.Should().NotBeEmpty();
            callBackUrl.Should().Be(url);
        }
    }
}