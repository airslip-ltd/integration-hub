using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.Shopify;

public class ShopifyPermanentAccess : PermanentAccessBase
{
    [JsonProperty(PropertyName = "client_id")]
    public sealed override string? AppId { get; set; }

    [JsonProperty(PropertyName = "client_secret")]
    public sealed override string? AppSecret { get; set; }

    [JsonProperty(PropertyName = "code")] 
    public sealed override string ShortLivedCode { get; set; }

    public ShopifyPermanentAccess(
        string appId,
        string appSecret,
        string shortLivedCode)
    {
        AppId = appId;
        AppSecret = appSecret;
        ShortLivedCode = shortLivedCode;
    }
}