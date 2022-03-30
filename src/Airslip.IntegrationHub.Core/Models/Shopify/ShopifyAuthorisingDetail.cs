using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.Shopify;

public class ShopifyAuthorisingDetail : ShortLivedAuthorisationDetail
{
    [JsonProperty(PropertyName = "code")]
    public override string ShortLivedCode { get; set; } = string.Empty;
        
    [JsonProperty(PropertyName = "shop")]
    public override string StoreName { get; set; } = string.Empty;
        
    public string Hmac { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}