using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models;

public class EbayPermanentAccess : PermanentAccessBase
{
    [JsonProperty(PropertyName = "grant_type")]
    public sealed override string? GrantType { get; set; } = "client_credentials";

    [JsonProperty(PropertyName = "code")] public sealed override string ShortLivedCode { get; set; }

    [JsonProperty(PropertyName = "redirect_uri")]
    public string RedirectUrl { get; set; }

    public EbayPermanentAccess(
        string shortLivedCode,
        string redirectUrl)
    {
        ShortLivedCode = shortLivedCode;
        RedirectUrl = redirectUrl;
    }
}