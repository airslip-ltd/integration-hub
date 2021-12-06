using Airslip.Common.Types.Configuration;
using Airslip.Common.Types.Enums;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Airslip.IntegrationHub.Services.UnitTests
{
    public class ExternalAuthServiceTests
    {
        private readonly ExternalAuthService _sut;

        public ExternalAuthServiceTests()
        {
            Mock<IOptions<SettingCollection<ProviderSetting>>> mockSettings = new();
            SettingCollection<ProviderSetting> settings = new() {Settings = new Dictionary<string, ProviderSetting>
            {
                {
                    "Vend", new ProviderSetting
                    {
                        BaseUri = "https://secure.vendhq.com/connect",
                        ClientId = "SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL",
                        ClientSecret = "yujZrOdVKZbGXUvfYP6VjWYluZJ77ge4",
                        RedirectUri = "http://localhost:38101/v1/auth"
                    }
                }
            }};

            mockSettings
                .Setup(s => s.Value)
                .Returns(settings);

            _sut = new ExternalAuthService(mockSettings.Object);
        }

        [Fact]
        public void Can_generate_callback_url()
        {
            string callBackUrl = _sut.GenerateCallbackUrl(PosProviders.Vend, "account-id");
            string url =
                "https://secure.vendhq.com/connect?response_type=code&client_id=SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL&redirect_uri=http://localhost:38101/v1/auth&state=account-id";

            callBackUrl.Should().NotBeEmpty();
            callBackUrl.Should().Be(url);
        }

        [Fact]
        public void Can_override_redirect_uri_for_callback_url_generator()
        {
            string callBackUrl = _sut.GenerateCallbackUrl(PosProviders.Vend, "account-id", "override-url");
            string url =
                "https://secure.vendhq.com/connect?response_type=code&client_id=SrSLyYuwnffktH2oGJEJbQTiCXzkHgoL&redirect_uri=override-url&state=account-id";

            callBackUrl.Should().NotBeEmpty();
            callBackUrl.Should().Be(url);
        }
    }
}