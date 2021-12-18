using Newtonsoft.Json;

namespace Airslip.IntegrationHub.Core.Models
{
    public record ShopifyProviderAuthorisation : ProviderAuthorisation
    {
        [JsonProperty(PropertyName = "access_token")]
        public override string Password { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "scope")]
        public override string AccessScope { get; set; } = string.Empty;
        public override string Login { get; set; } = string.Empty;
    }
}
