using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models
{
    public class EbayAuthorisingDetail : ShortLivedAuthorisationDetail
    {
        [JsonProperty(PropertyName = "code")]
        public override string ShortLivedCode { get; set; } = string.Empty;
        
        [JsonProperty(PropertyName = "state")]
        public override string EncryptedUserInfo { get; set; } = string.Empty;
        
        public string Error { get; set; } = string.Empty;
    }
}
