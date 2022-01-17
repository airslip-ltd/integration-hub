using Airslip.Common.Security.Configuration;
using Airslip.Common.Testing;
using Airslip.Common.Types.Configuration;
using Airslip.IntegrationHub.Core;
using Airslip.IntegrationHub.Core.Models;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Airslip.IntegrationHub.Services.UnitTests
{
    public class ProviderAuthorisingMappingTests
    {
        private readonly Mock<IOptions<EncryptionSettings>> _encryptionSettings;
        private readonly MapperConfiguration _config;
        private readonly IMapper _mapper;
        
        public ProviderAuthorisingMappingTests()
        {
            string projectName = "Airslip.IntegrationHub";
            _encryptionSettings = OptionsMock.SetUpOptionSettings<EncryptionSettings>(projectName)!;
            SettingCollection<ProviderSetting> settings = Factory.ProviderConfiguration;
            
            _config = new MapperConfiguration(cfg =>
            {
                cfg.AddShopify(settings);
            });
            
            _mapper = new Mapper(_config);
        }

        [Fact]
        public void Can_map_shopify_authorising_callback_to_common_model()
        {
            ShopifyProviderAuthorisingDetail shopifyAuthDetail = new()
            {
                Code = "9eb34b1a83917cee25bb0199c8711bab",
                Hmac = "24bd4063d10d51f5f117f8f9e936412cbc71a049400d3dd58b0407c8737b1bf3",
                Host = "YWlyc2xpcC1kZXZlbG9wbWVudC5teXNob3BpZnkuY29tL2FkbWlu",
                Shop = "airslip-development.myshopify.com",
                State = "b951eMbRF6NelKyGXt8cRaj%2Fflv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV%2F5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1",
                Timestamp = 1639833474
            };
            
            ProviderAuthorisingDetail result = _mapper.Map<ProviderAuthorisingDetail>(shopifyAuthDetail);

            result.StoreName.Should().Be("airslip-development");
            result.EncryptedUserInfo.Should().Be("b951eMbRF6NelKyGXt8cRaj%2Fflv3G2GKHQ3N0vhPQhscLKW2bk6JoOc5rS4EzFP7MV%2F5ugljPQikkfowmDsZpomRpwieoZ41TMIgMu2H0nGx77YHnhearD2hFNkOvGd1");
            result.BaseUri.Should().Be("https://airslip-development.myshopify.com");
            result.ShortLivedCode.Should().Be("9eb34b1a83917cee25bb0199c8711bab");
            result.PermanentAccessUrl.Should().Be("https://airslip-development.myshopify.com/admin/oauth/access_token");
        }
    }
}