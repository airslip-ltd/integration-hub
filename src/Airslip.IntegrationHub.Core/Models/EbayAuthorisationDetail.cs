using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models
{
    public class EbayAuthorisationDetail : BasicAuthorisationDetail
    {
        [JsonProperty(PropertyName = "refresh_token")]
        public override string Password { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "scope")]
        public override string AccessScope { get; set; } = string.Empty;
        // [JsonProperty(PropertyName = "refresh_token")]
        // public override string Login { get; set; } = string.Empty;
    }
}
