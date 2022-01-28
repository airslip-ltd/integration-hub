using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models;

public class EbayPermanentAccess : PermanentAccessBase
{
    [JsonProperty(PropertyName = "grant_type")]
    public sealed override string? GrantType { get; set; } = "authorization_code";

    [JsonProperty(PropertyName = "code")] public sealed override string ShortLivedCode { get; set; }

    [JsonProperty(PropertyName = "redirect_uri")]
    public string RedirectUrl { get; set; }
    
    [JsonProperty(PropertyName = " ebay_site_id")]
    public string SiteId { get; set; }

    public EbayPermanentAccess(
        string shortLivedCode,
        string redirectUrl,
        string siteId)
    {
        ShortLivedCode = shortLivedCode;
        RedirectUrl = redirectUrl;
        SiteId = siteId;
    }
}