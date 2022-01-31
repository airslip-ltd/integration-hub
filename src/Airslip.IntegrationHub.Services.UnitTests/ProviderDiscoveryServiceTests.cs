using Airslip.Common.Testing;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
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
    [InlineData(PosProviders.Shopify, $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData(PosProviders.EtsyAPIv3, $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData(PosProviders.WoocommerceApi, $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData(PosProviders.EBay, $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData(PosProviders.Squarespace, $"{nameof(PosProviders.Api2Cart)}")]
    [InlineData(PosProviders.BigcommerceApi, $"{nameof(PosProviders.Api2Cart)}")]
    public void Can_get_provider_details(PosProviders provider, string expectedDestination)
    {
        ProviderDetails providerDetails = _sut.GetProviderDetails(provider);
        providerDetails.Provider.Should().Be(provider);
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
    [InlineData(PosProviders.Shopify, "https://{0}")]
    [InlineData(PosProviders.EtsyAPIv3, "https://www.etsy.com")]
    [InlineData(PosProviders.WoocommerceApi, "https://{0}")]
    [InlineData(PosProviders.EBay, "https://api.sandbox.ebay.com")]
    [InlineData(PosProviders.Squarespace, "https://{0}.squarespace.com")]
    [InlineData(PosProviders.BigcommerceApi, "https://{0}.mybigcommerce.com")]
    public void Can_get_providers_default_base_uri(PosProviders provider, string expectedBaseUri)
    {
        ProviderDetails providerDetails = _sut.GetProviderDetails(provider);
        providerDetails.ProviderSetting.BaseUri.Should().Be(expectedBaseUri);
    }
}