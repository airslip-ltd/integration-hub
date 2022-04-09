using Airslip.Common.Types.Configuration;
using Airslip.IntegrationHub.Core.Enums;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using System.Collections.Generic;
using System.Net;

namespace Airslip.IntegrationHub.Core.Common;   

public record IntegrationSetting
{
    public string PublicApiSettingName { get; init; } = string.Empty;
    public PublicApiSetting PublicApiSetting { get; set; } = new();
    public string AuthorisationRouteFormat { get; init; } = string.Empty;
    public AuthorisationRouteType AuthorisationRouteType { get; init; } = AuthorisationRouteType.Internal;
    public string AuthoriseRouteFormat { get; init; } = string.Empty;
    public SourceType SourceType { get; init; }
    public bool AuthorisePassthrough { get; init; } = false;
    public bool AnonymousUsage { get; init; } = false;
    public bool OAuthRedirect { get; init; } = false;
    public bool RequiresStoreName { get; set; }
    public bool RequireUrlEncode { get; set; }
    public string Version { get; set; } = string.Empty;
    public AuthenticationSchemes? AuthoriseScheme { get; set; } = AuthenticationSchemes.None;
    public bool AuthoriseHeadersRequired { get; set; }
    public string AuthorisationBaseUri { get; set; } = string.Empty;

    public string? AuthoriseBaseUri { get; set; }

    public string ApiKey { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string ReturnPageFormat { get; set; } = string.Empty;
    public string ReturnPage { get; private set; } = string.Empty;
    public ProviderAuthStrategy AuthStrategy { get; set; }
    public IntegrationTypes IntegrationType { get; set; }
    public List<AuthRequestTypes> HmacValidateOn { get; set; } = new();
    public string? Environment { get; set; }
    public string? Location { get; set; }
    public string? AdditionalFieldOne { get; set; }
    public string? AdditionalFieldTwo { get; set; }
    public string? AdditionalFieldThree { get; set; }
    public MethodTypes ExchangeCodeMethodType { get; set; } = MethodTypes.POST;
    public AuthorisationParameterNames AuthorisationParameterNames { get; set; } = new();
  

    public void FormatReturnPage(string baseUri)
    {
        ReturnPage = $"{baseUri}/{ReturnPageFormat}";
    }
    
    public bool ShouldValidateHmac(AuthRequestTypes authRequestType)
    {
        return HmacValidateOn.Contains(authRequestType);
    }

    public bool ValidateIfRequiresStoreName(string? storeName)
    {
        return RequiresStoreName && string.IsNullOrWhiteSpace(storeName);
    }

    public bool IsNotSupported()
    {
        return string.IsNullOrEmpty(PublicApiSettingName);
    }

    public string FormatBaseUri(string shop)
    {
        return AuthorisationBaseUri.Replace("{shop}", shop);
    }
}

public class AuthorisationParameterNames
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string AccessScope { get; set; } = string.Empty;
    public string Shop { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}