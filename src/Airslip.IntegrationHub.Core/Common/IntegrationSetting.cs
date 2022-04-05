using Airslip.IntegrationHub.Core.Enums;
using Airslip.IntegrationHub.Core.Interfaces;
using Airslip.IntegrationHub.Core.Models;
using System.Collections.Generic;

namespace Airslip.IntegrationHub.Core.Common;

public record IntegrationSetting
{
    public string PublicApiSettingName { get; init; } = string.Empty;
    public string AuthorisationRouteFormat { get; init; } = string.Empty;
    public AuthorisationRouteType AuthorisationRouteType { get; init; } = AuthorisationRouteType.Internal;
    public string AuthoriseRouteFormat { get; init; } = string.Empty;
    public string DeleteRouteFormat { get; init; } = string.Empty;
    public SourceType SourceType { get; init; }
    public bool AuthorisePassthrough { get; init; } = false;
    public bool AnonymousUsage { get; init; } = false;
    public bool OAuthRedirect { get; init; } = false;
    public bool RequiresStoreName { get; set; }
    public string AuthorisationBaseUri { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public ProviderAuthStrategy AuthStrategy { get; set; }
    public List<AuthRequestTypes> HmacValidateOn { get; set; } = new();
}