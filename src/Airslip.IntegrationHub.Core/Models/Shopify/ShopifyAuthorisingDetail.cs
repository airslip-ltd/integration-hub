using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.Shopify;

public class ShopifyAuthorisingDetail : ShortLivedAuthorisationDetail
{
    [JsonProperty(PropertyName = "code")]
    public override string ShortLivedCode { get; set; } = string.Empty;
        
    [JsonProperty(PropertyName = "state")]
    public string StateEncryptedUserInfo { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName = "user_info")]
    public string? PassedEncryptedUserInfo { get; set; }

    public override string EncryptedUserInfo
    {
        get => PassedEncryptedUserInfo ?? StateEncryptedUserInfo;
        set => StateEncryptedUserInfo = value;
    }

    [JsonProperty(PropertyName = "shop")]
    public override string StoreName { get; set; } = string.Empty;
        
    public string Hmac { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}