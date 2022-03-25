using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models.WooCommerce
{
    public class WooCommerceAuthorisationDetail : BasicAuthorisationDetail
    {
        [JsonProperty(PropertyName = "key_id")]
        public string KeyId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "user_id")]
        public override string EncryptedUserInfo { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "consumer_key")]
        public override string Login { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "consumer_secret")] 
        public override string Password { get; set; } = string.Empty;
        
        [JsonProperty(PropertyName = "key_permissions")] 
        public override string AccessScope { get; set; } = string.Empty;
    }
}
