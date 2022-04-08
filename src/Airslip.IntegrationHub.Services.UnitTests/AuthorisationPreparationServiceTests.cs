using Airslip.Common.Security.Configuration;
using Airslip.Common.Testing;
using Airslip.Common.Types.Configuration;
using Airslip.Common.Utilities;
using Airslip.IntegrationHub.Core.Common.Discovery;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using Xunit;
#pragma warning disable CS8618

namespace Airslip.IntegrationHub.Services.UnitTests;

public class AuthorisationPreparationServiceTests
{
    private readonly AuthorisationPreparationService _sut;

    public AuthorisationPreparationServiceTests()
    {
        string projectName = "Airslip.IntegrationHub";
        Mock<IOptions<SettingCollection<ProviderSetting>>> providerSettingsMock = new();
        Mock<IIntegrationDiscoveryService> integrationDiscoveryServiceMock = new();
        Mock<ISensitiveInformationService> sensitiveInformationServiceMock = new();
        Mock<IOptions<EncryptionSettings>> EncryptionSettingsMock = OptionsMock.SetUpOptionSettings<EncryptionSettings>(projectName)!;
        
        IConfiguration appSettingsConfig = OptionsMock.InitialiseConfiguration(projectName)!;
        SettingCollection<ProviderSetting> providerSettings = new();
        appSettingsConfig.GetSection("ProviderSettings").Bind(providerSettings);

        providerSettingsMock
            .Setup(s => s.Value)
            .Returns(providerSettings);

        _sut = new AuthorisationPreparationService(EncryptionSettingsMock.Object, integrationDiscoveryServiceMock.Object, sensitiveInformationServiceMock.Object);
    }
    
    private void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
    {
        string currentError = errorArgs.ErrorContext.Error.Message;
        errorArgs.ErrorContext.Handled = true;
    }
    
    [Fact]
    public void A()
    {
        // arrange

        string successContent =
            "{\"access_token\":\"secret_3ETSQDPV3c86bqBgZWRut2LnUr9QeBi3\",\"token_type\":\"Bearer\",\"scope\":\"read_store_profile read_orders read_catalog read_invoices\",\"store_id\":71467012,\"user_id\":71467012,\"email\":\"tmcdonough@airslip.com\" }";
        string failedContent = "{\"access_token\":\"secret_3ETSQDPV3c86bqBgZWRut2LnUr9QeBi3\",\"token_type\":\"Bearer\",\"scope\":\"read_store_profile read_orders read_catalog read_invoices\",\"store_id\":71467012,\"user_id\":71467012,\"admin_sso\":{\"role\":\"STORE_OWNER\"},\"email\":\"tmcdonough@airslip.com\"}\n";
        
        var settings = new JsonSerializerSettings
        {
            Error = (se, ev) => { ev.ErrorContext.Handled = true; }
        };
        var dict =   JsonConvert.DeserializeObject<Dictionary<string, object>>(failedContent, settings);

        dict.Should().NotBeNull();
        successContent.Should().NotBeNull();

        //Dictionary<string, string> parameters = Json.Deserialize<Dictionary<string, string>>(content);


        // act
        //ICollection<KeyValuePair<string, string>> queryParams = _sut.QueryStringReplacer();

        // asset

    }
    
    public class FakeHttpRequestData : HttpRequestData
    {
        public FakeHttpRequestData(FunctionContext functionContext, Uri url, Stream? body = null) : base(functionContext)
        {
            Url = url;
            Body = body ?? new MemoryStream();
        }

        public override Stream Body { get; } = new MemoryStream();

        public override HttpHeadersCollection Headers { get; } = new();

        public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
        public override Uri Url { get; }

        public override IEnumerable<ClaimsIdentity> Identities { get; }

        public override string Method { get; }

        public override HttpResponseData CreateResponse()
        {
            return new FakeHttpResponseData(FunctionContext);
        }
    }

    public class FakeHttpResponseData : HttpResponseData
    {
        public FakeHttpResponseData(FunctionContext functionContext) : base(functionContext)
        {
        }

        public override HttpStatusCode StatusCode { get; set; }
        public override HttpHeadersCollection Headers { get; set; } = new();
        public override Stream Body { get; set; } = new MemoryStream();
        public override HttpCookies Cookies { get; }
    }
}