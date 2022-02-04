using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.Shopify;

public class ShopifyPermanentAccess : PermanentAccessBase
{
    [JsonProperty(PropertyName = "client_id")]
    public sealed override string? ApiKey { get; set; }

    [JsonProperty(PropertyName = "client_secret")]
    public sealed override string? ApiSecret { get; set; }

    [JsonProperty(PropertyName = "code")] 
    public sealed override string ShortLivedCode { get; set; }

    public ShopifyPermanentAccess(
        string apiKey,
        string apiSecret,
        string shortLivedCode)
    {
        ApiKey = apiKey;
        ApiSecret = apiSecret;
        ShortLivedCode = shortLivedCode;
    }
}