using Airslip.Common.Testing;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.Common.Utilities.Extensions;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Airslip.IntegrationHub.Services.UnitTests;

public class ProviderDiscoveryServiceTests
{
    private readonly ProviderDiscoveryService _sut;

    public ProviderDiscoveryServiceTests()
    {
        string projectName = "Airslip.IntegrationHub";
        Mock<IOptions<SettingCollection<ProviderSetting>>> providerSettingsMock = new();
        Mock<IOptions<PublicApiSettings>> publicApiSettingsMock =
            OptionsMock.SetUpOptionSettings<PublicApiSettings>(projectName)!;

        IConfiguration appSettingsConfig = OptionsMock.InitialiseConfiguration(projectName)!;

        SettingCollection<ProviderSetting> providerSettings = new();
        appSettingsConfig.GetSection("ProviderSettings").Bind(providerSettings);
        
        providerSettingsMock
            .Setup(s => s.Value)
            .Returns(providerSettings);

        _sut = new ProviderDiscoveryService(
            providerSettingsMock.Object,
            publicApiSettingsMock.Object);
    }

    // Step 1: Add new provider to test
    [Theory]
    [InlineData("Shopify", $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData("EtsyAPIv3", $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData("WoocommerceApi", $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData("EBay", $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData("Squarespace", $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData("BigcommerceApi", $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData("_3DCart", $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData("Ecwid", $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData("AmazonSP", $"{nameof(PosProviders.Api2Cart)}")]
    public void Can_get_provider_details(string provider, string expectedDestination)
    {
        ProviderDetails providerDetails = _sut.GetPosProviderDetails(provider)!;
        providerDetails.Provider.Should().Be(provider.ParseIgnoreCase<PosProviders>());
        providerDetails.ProviderSetting.BaseUri.Should().NotBeEmpty();
        providerDetails.ProviderSetting.MiddlewareDestinationAppName.Should().Be(expectedDestination);
        providerDetails.CallbackRedirectUri.Should().NotBeEmpty();
        providerDetails.MiddlewareDestinationBaseUri.Should().NotBeEmpty();
        providerDetails.PublicApiSetting.UriSuffix.Should().NotBeEmpty();
        providerDetails.PublicApiSetting.BaseUri.Should().NotBeEmpty();
        providerDetails.PublicApiSetting.Version.Should().NotBeEmpty();
        providerDetails.PublicApiSetting.ApiKey.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("Shopify", "https://{0}")]
    [InlineData("EtsyAPIv3", "https://www.etsy.com")]
    [InlineData("WoocommerceApi", "https://{0}")]
    [InlineData("EBay", "https://api.sandbox.ebay.com")]
    [InlineData("Squarespace", "https://api.squarespace.com")]
    [InlineData("BigcommerceApi", "https://{0}.mybigcommerce.com")]
    [InlineData("_3DCart", "https://{0}.3dcart.com")]
    [InlineData("Ecwid", "https://app.ecwid.com/api/v3/{0}")]
    public void Can_get_providers_default_base_uri(string provider, string expectedBaseUri)
    {
        ProviderDetails providerDetails = _sut.GetPosProviderDetails(provider)!;
        providerDetails.ProviderSetting.BaseUri.Should().Be(expectedBaseUri);
    }
}