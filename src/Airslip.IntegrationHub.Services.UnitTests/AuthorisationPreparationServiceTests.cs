using Airslip.Common.Testing;
using Airslip.Common.Types.Configuration;
using Airslip.IntegrationHub.Core.Implementations;
using Airslip.IntegrationHub.Core.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
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
        Mock<IOptions<PublicApiSettings>> publicApiSettingsMock = OptionsMock.SetUpOptionSettings<PublicApiSettings>(projectName)!;
        
        IConfiguration appSettingsConfig = OptionsMock.InitialiseConfiguration(projectName)!;
        SettingCollection<ProviderSetting> providerSettings = new();
        appSettingsConfig.GetSection("ProviderSettings").Bind(providerSettings);

        providerSettingsMock
            .Setup(s => s.Value)
            .Returns(providerSettings);

        _sut = new AuthorisationPreparationService();
    }
    
    [Fact]
    public void A()
    {
        
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