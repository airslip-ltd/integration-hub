using Airslip.IntegrationHub.Core.Enums;
using Airslip.IntegrationHub.Core.Models;

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
    public ProviderSetting ProviderSetting { get; init; } = new();
}