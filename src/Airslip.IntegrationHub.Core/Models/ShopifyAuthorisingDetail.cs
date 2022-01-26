using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models
{
    public class ShopifyAuthorisingDetail : ShortLivedAuthorisationDetail
    {
        [JsonProperty(PropertyName = "code")]
        public override string ShortLivedCode { get; set; } = string.Empty;
        
        [JsonProperty(PropertyName = "state")]
        public override string EncryptedUserInfo { get; set; } = string.Empty;
        public string Shop { get; set; } = string.Empty;
        public string Hmac { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }
}
